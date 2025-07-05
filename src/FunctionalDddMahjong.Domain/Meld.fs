namespace FunctionalDddMahjong.Domain

// 面子（メンツ）管理モジュール
module Meld =
    open Tile

    // 面子の種類を表現する型（3牌の組み合わせのみ）
    type MeldType =
        | Sequence of Tile * Tile * Tile // 順子：連続する3牌（例：1-2-3萬）
        | Triplet of Tile * Tile * Tile // 刻子：同じ牌3つ（例：3-3-3筒）

    // 面子を表現する型（Value Object）
    type Meld = private Meld of MeldType

    // 面子作成関数（スマートコンストラクタ）
    let private createMeld meldType = Meld meldType

    // 面子の内容を取得
    let getMeldValue (Meld meldType) = meldType

    // 面子に含まれる牌をリストで取得
    let getMeldTiles meld =
        match getMeldValue meld with
        | Sequence(t1, t2, t3) -> [ t1; t2; t3 ]
        | Triplet(t1, t2, t3) -> [ t1; t2; t3 ]

    // エラー型定義
    type MeldError =
        | NotSequence of Tile list
        | NotTriplet of Tile list
        | InsufficientTiles of int

    // 数字牌が連続しているかチェック
    let private isConsecutiveNumbers n1 n2 n3 =
        let values =
            [ n1; n2; n3 ]
            |> List.map getNumberOrder
            |> List.sort

        match values with
        | [ a; b; c ] -> b = a + 1 && c = b + 1
        | _ -> false

    // 順子検出関数
    let tryCreateSequence tiles =
        match tiles with
        | [ t1; t2; t3 ] ->
            let sortedTiles =
                List.sortWith Tile.compare [ t1; t2; t3 ]

            match sortedTiles |> List.map getValue with
            | [ Character n1; Character n2; Character n3 ] when isConsecutiveNumbers n1 n2 n3 ->
                Ok(createMeld (Sequence(List.item 0 sortedTiles, List.item 1 sortedTiles, List.item 2 sortedTiles)))
            | [ Circle n1; Circle n2; Circle n3 ] when isConsecutiveNumbers n1 n2 n3 ->
                Ok(createMeld (Sequence(List.item 0 sortedTiles, List.item 1 sortedTiles, List.item 2 sortedTiles)))
            | [ Bamboo n1; Bamboo n2; Bamboo n3 ] when isConsecutiveNumbers n1 n2 n3 ->
                Ok(createMeld (Sequence(List.item 0 sortedTiles, List.item 1 sortedTiles, List.item 2 sortedTiles)))
            | _ -> Error(NotSequence tiles)
        | _ -> Error(InsufficientTiles(List.length tiles))

    // 刻子検出関数
    let tryCreateTriplet tiles =
        match tiles with
        | [ t1; t2; t3 ] ->
            if t1 = t2 && t2 = t3 then
                Ok(createMeld (Triplet(t1, t2, t3)))
            else
                Error(NotTriplet tiles)
        | _ -> Error(InsufficientTiles(List.length tiles))

    // 牌のリストから可能な面子を全て検出（順子と刻子を統合）
    let findAllMelds (tiles: Tile list) : Meld list =
        let sequences =
            let rec findSequencesRec remaining acc =
                match remaining with
                | t1 :: t2 :: t3 :: rest ->
                    match tryCreateSequence [ t1; t2; t3 ] with
                    | Ok sequence ->
                        // 見つかった順子を除いた残りの牌で続行
                        let seqTiles = getMeldTiles sequence

                        let newRemaining =
                            seqTiles
                            |> List.fold
                                (fun tiles tile ->
                                    let rec removeFirst =
                                        function
                                        | [] -> []
                                        | h :: t when h = tile -> t
                                        | h :: t -> h :: removeFirst t

                                    removeFirst tiles)
                                remaining

                        findSequencesRec newRemaining (sequence :: acc)
                    | Error _ -> findSequencesRec (t2 :: t3 :: rest) acc
                | _ -> acc

            let sortedTiles =
                List.sortWith Tile.compare tiles

            findSequencesRec sortedTiles []

        let triplets =
            let groupedTiles =
                tiles
                |> List.groupBy id
                |> List.filter (fun (_, group) -> List.length group >= 3)
                |> List.map (fun (tile, _) -> tile)

            groupedTiles
            |> List.choose (fun tile ->
                match tryCreateTriplet [ tile; tile; tile ] with
                | Ok triplet -> Some triplet
                | Error _ -> None)

        sequences @ triplets

    // 面子の種類を判定
    let getMeldType meld =
        match getMeldValue meld with
        | Sequence _ -> "Sequence"
        | Triplet _ -> "Triplet"

    // 面子を文字列で表現（デバッグ用）
    let meldToString meld =
        let tiles = getMeldTiles meld

        let tileStrings =
            tiles |> List.map Tile.toString

        let meldTypeStr = getMeldType meld
        sprintf "%s: [%s]" meldTypeStr (String.concat ", " tileStrings)

    // 面子を短縮表記で表現（例: "234m", "111p", "EEE"）
    let meldToShortString meld =
        match getMeldValue meld with
        | Sequence(t1, t2, t3) ->
            let numbers =
                [ t1; t2; t3 ]
                |> List.map (fun t ->
                    match Tile.getValue t with
                    | Character n -> string (Tile.getNumberOrder n)
                    | Circle n -> string (Tile.getNumberOrder n)
                    | Bamboo n -> string (Tile.getNumberOrder n)
                    | _ -> "")
                |> String.concat ""

            let suffix =
                match Tile.getValue t1 with
                | Character _ -> "m"
                | Circle _ -> "p"
                | Bamboo _ -> "s"
                | _ -> ""

            numbers + suffix

        | Triplet(t1, _, _) ->
            match Tile.getValue t1 with
            | Character n ->
                let num = string (Tile.getNumberOrder n)
                num + num + num + "m"
            | Circle n ->
                let num = string (Tile.getNumberOrder n)
                num + num + num + "p"
            | Bamboo n ->
                let num = string (Tile.getNumberOrder n)
                num + num + num + "s"
            | Honor East -> "EEE"
            | Honor South -> "SSS"
            | Honor West -> "WWW"
            | Honor North -> "NNN"
            | Honor White -> "WHWHWH"
            | Honor Green -> "GRGRGR"
            | Honor Red -> "RDRDRD"
