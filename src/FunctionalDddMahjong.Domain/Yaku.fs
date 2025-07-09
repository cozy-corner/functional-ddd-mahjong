namespace FunctionalDddMahjong.Domain

// 麻雀の役（Yaku）管理モジュール
module Yaku =
    open Tile
    open Meld
    open Pair
    open Hand

    // 役の型定義
    type Yaku =
        | Tanyao // 断么九（中張牌のみ）
        | Pinfu // 平和（順子4つ+数牌の雀頭）- 簡略版
        | Toitoi // 対々和（刻子4つ+雀頭）

    // 役判定のエラー型（真のエラーのみ）
    type YakuError = InvalidHandState of string

    // ヘルパー関数: 牌が中張牌（2-8の数牌）かどうか判定
    let private isSimple tile =
        match Tile.getValue tile with
        | TileType.Character value
        | TileType.Circle value
        | TileType.Bamboo value ->
            match value with
            | NumberValue.Two
            | NumberValue.Three
            | NumberValue.Four
            | NumberValue.Five
            | NumberValue.Six
            | NumberValue.Seven
            | NumberValue.Eight -> true
            | _ -> false
        | TileType.Honor _ -> false

    // ヘルパー関数: 牌が么九牌（1, 9, 字牌）かどうか判定
    let private isTerminalOrHonor tile =
        match Tile.getValue tile with
        | TileType.Character value
        | TileType.Circle value
        | TileType.Bamboo value ->
            match value with
            | NumberValue.One
            | NumberValue.Nine -> true
            | _ -> false
        | TileType.Honor _ -> true

    // ヘルパー関数: 分解パターンから全ての牌を取得
    let private getAllTilesFromDecomposition (melds: Meld.Meld list, pair: Pair.Pair) =
        let meldTiles =
            melds
            |> List.collect (fun meld ->
                match Meld.getMeldValue meld with
                | Meld.Sequence(t1, t2, t3) -> [ t1; t2; t3 ]
                | Meld.Triplet(t1, t2, t3) -> [ t1; t2; t3 ])

        let pairTiles = Pair.getPairTiles pair
        meldTiles @ pairTiles

    // タンヤオ（断么九）の判定
    // 全ての牌が中張牌（2-8の数牌）である必要がある
    let checkTanyao (winningHand: Hand.WinningHand) : Option<Yaku> =
        let decompositions =
            Hand.getDecompositions winningHand

        match decompositions with
        | (melds, pair) :: _ -> // 任意の分解パターン（牌は同じ）
            let allTiles =
                getAllTilesFromDecomposition (melds, pair)

            if allTiles |> List.forall isSimple then
                Some Tanyao
            else
                None
        | [] -> None // This should never happen for a valid WinningHand

    // ピンフ（平和）の判定 - 簡略版
    // 全ての面子が順子で、雀頭が数牌である必要がある
    let checkPinfu (winningHand: Hand.WinningHand) : Option<Yaku> =
        let decompositions =
            Hand.getDecompositions winningHand

        match decompositions with
        | (melds, pair) :: _ ->
            // 全ての面子が順子かチェック
            let allSequences =
                melds
                |> List.forall (fun meld ->
                    match Meld.getMeldValue meld with
                    | Meld.Sequence _ -> true
                    | Meld.Triplet _ -> false)

            // 雀頭が数牌かチェック
            let pairTiles = Pair.getPairTiles pair

            let pairIsNumeric =
                match pairTiles with
                | [ tile1; _ ] ->
                    match Tile.getValue tile1 with
                    | TileType.Honor _ -> false
                    | _ -> true
                | _ -> false

            if allSequences && pairIsNumeric then
                Some Pinfu
            else
                None
        | [] -> None

    // トイトイ（対々和）の判定
    // 全ての面子が刻子である必要がある
    let checkToitoi (winningHand: Hand.WinningHand) : Option<Yaku> =
        let decompositions =
            Hand.getDecompositions winningHand

        match decompositions with
        | (melds, _) :: _ ->
            // 全ての面子が刻子かチェック
            let allTriplets =
                melds
                |> List.forall (fun meld ->
                    match Meld.getMeldValue meld with
                    | Meld.Sequence _ -> false
                    | Meld.Triplet _ -> true)

            if allTriplets then
                Some Toitoi
            else
                None
        | [] -> None

    // 役の名前を取得
    let getName =
        function
        | Tanyao -> "断么九"
        | Pinfu -> "平和"
        | Toitoi -> "対々和"

    // 役の翻数を取得
    let getHan =
        function
        | Tanyao -> 1
        | Pinfu -> 1
        | Toitoi -> 2

    // 役の英語名を取得
    let getEnglishName =
        function
        | Tanyao -> "All Simples"
        | Pinfu -> "All Sequences"
        | Toitoi -> "All Triplets"
