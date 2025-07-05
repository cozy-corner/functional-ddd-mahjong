namespace FunctionalDddMahjong.Domain

// ペア（対子）管理モジュール
module Pair =
    open Tile

    // ペアの種類を表現する型（2牌のペア）
    type PairType = Pair of Tile * Tile // ペア：同じ牌2つ（例：東-東）

    // ペアを表現する型（Value Object）
    type Pair = private Pair of PairType

    // ペア作成関数（スマートコンストラクタ）
    let private createPair pairType = Pair pairType

    // ペアの内容を取得
    let getPairValue (Pair pairType) = pairType

    // ペアに含まれる牌をリストで取得
    let getPairTiles pair =
        match getPairValue pair with
        | PairType.Pair(t1, t2) -> [ t1; t2 ]

    // エラー型定義
    type PairError =
        | NotPair of Tile list
        | InsufficientTiles of int

    // ペア検出関数
    let tryCreatePair tiles =
        match tiles with
        | [ t1; t2 ] ->
            if t1 = t2 then
                Ok(createPair (PairType.Pair(t1, t2)))
            else
                Error(NotPair tiles)
        | _ -> Error(InsufficientTiles(List.length tiles))

    // 牌のリストから可能なペアを全て検出
    let findAllPairs (tiles: Tile list) : Pair list =
        let groupedTiles =
            tiles
            |> List.groupBy id
            |> List.filter (fun (_, group) -> List.length group >= 2)
            |> List.map (fun (tile, _) -> tile)

        groupedTiles
        |> List.choose (fun tile ->
            match tryCreatePair [ tile; tile ] with
            | Ok pair -> Some pair
            | Error _ -> None)

    // ペアの種類を判定
    let getPairType pair = "Pair"

    // ペアを文字列で表現（デバッグ用）
    let pairToString pair =
        let tiles = getPairTiles pair

        let tileStrings =
            tiles |> List.map Tile.toString

        let pairTypeStr = getPairType pair
        sprintf "%s: [%s]" pairTypeStr (String.concat ", " tileStrings)

    // ペアを短縮表記で表現（例: "22m", "NN"）
    let pairToShortString pair =
        let tiles = getPairTiles pair

        match tiles with
        | [ t1; _ ] ->
            match Tile.getValue t1 with
            | Character n ->
                let num = string (Tile.getNumberOrder n)
                num + num + "m"
            | Circle n ->
                let num = string (Tile.getNumberOrder n)
                num + num + "p"
            | Bamboo n ->
                let num = string (Tile.getNumberOrder n)
                num + num + "s"
            | Honor East -> "EE"
            | Honor South -> "SS"
            | Honor West -> "WW"
            | Honor North -> "NN"
            | Honor White -> "WHWH"
            | Honor Green -> "GRGR"
            | Honor Red -> "RDRD"
        | _ -> ""
