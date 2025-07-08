namespace FunctionalDddMahjong.Domain

// 手牌管理モジュール
module Hand =
    open Tile

    // 手牌のエラー型定義
    type HandError =
        | InvalidTileCount of actual: int * expected: string
        | TileNotFound of Tile

    // 手牌を表現する型（判別共用体）
    // 型レベルで13牌と14牌を区別
    type Hand =
        | Waiting of Tile list // 13牌（ツモ待ち）
        | Ready of Tile list // 14牌（打牌可能）

    // 手牌の内容を取得（共通操作）
    let getTiles =
        function
        | Waiting tiles
        | Ready tiles -> tiles

    // 手牌内の牌の数を取得
    let count hand = hand |> getTiles |> List.length

    // 牌のリストから待ち手牌を作成（配牌時：必ず13牌）
    let tryCreateFromDeal tiles =
        let tileCount = List.length tiles

        if tileCount = 13 then
            Ok(Waiting tiles)
        else
            Error(InvalidTileCount(tileCount, "13 (initial deal)"))

    // ツモ（13牌→14牌、型安全）
    let draw tile (Waiting tiles) = Ready(tile :: tiles)

    // 打牌（14牌→13牌、型安全）
    let discard tile (Ready tiles) =
        let rec removeFirst =
            function
            | [] -> None
            | h :: t when h = tile -> Some t
            | h :: t ->
                match removeFirst t with
                | Some remaining -> Some(h :: remaining)
                | None -> None

        match removeFirst tiles with
        | Some remaining -> Ok(Waiting remaining)
        | None -> Error(TileNotFound tile)

    // 手牌をソート（共通操作）
    let sort =
        function
        | Waiting tiles -> Waiting(List.sortWith Tile.compare tiles)
        | Ready tiles -> Ready(List.sortWith Tile.compare tiles)

    // 牌が手牌に含まれているか確認（共通操作）
    let contains tile hand =
        let tiles = getTiles hand
        List.contains tile tiles

    // 特定の牌の枚数を数える（共通操作）
    let countTile tile hand =
        let tiles = getTiles hand
        tiles |> List.filter ((=) tile) |> List.length

    // 手牌の状態判定（パターンマッチで型安全）
    let isWaiting =
        function
        | Waiting _ -> true
        | Ready _ -> false

    let isReady =
        function
        | Ready _ -> true
        | Waiting _ -> false

    // 手牌を全ての可能な4面子1雀頭パターンに分解する
    // 分解可能な全てのパターンをリストで返す（分解不可の場合は空リスト）
    let tryDecomposeAll hand =
        match hand with
        | Waiting _ -> [] // 13牌では分解不可
        | Ready tiles ->
            // 内部実装はMeldDecompositionモジュールに委譲
            MeldDecomposition.tryDecomposeAllInternal tiles

    // 手牌が和了形（4面子1雀頭に分解可能）かを判定する
    let isWinningHand hand =
        match tryDecomposeAll hand with
        | [] -> false // 分解パターンが存在しない = ノーテン
        | _ -> true // 1つ以上の分解パターンが存在 = 和了
