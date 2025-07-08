namespace FunctionalDddMahjong.Domain

// 麻雀の役（Yaku）管理モジュール
module Yaku =
    open Tile
    open Meld
    open Pair
    open Hand

    // 役の型定義
    type Yaku =
        | Tanyao    // 断么九（中張牌のみ）

    // 役判定のエラー型（真のエラーのみ）
    type YakuError =
        | InvalidHandState of string

    // ヘルパー関数: 牌が中張牌（2-8の数牌）かどうか判定
    let private isSimple tile =
        match Tile.getValue tile with
        | TileType.Character value | TileType.Circle value | TileType.Bamboo value ->
            match value with
            | NumberValue.Two | NumberValue.Three | NumberValue.Four | NumberValue.Five | NumberValue.Six | NumberValue.Seven | NumberValue.Eight -> true
            | _ -> false
        | TileType.Honor _ -> false

    // ヘルパー関数: 牌が么九牌（1, 9, 字牌）かどうか判定
    let private isTerminalOrHonor tile =
        match Tile.getValue tile with
        | TileType.Character value | TileType.Circle value | TileType.Bamboo value ->
            match value with
            | NumberValue.One | NumberValue.Nine -> true
            | _ -> false
        | TileType.Honor _ -> true

    // ヘルパー関数: 分解パターンから全ての牌を取得
    let private getAllTilesFromDecomposition (melds: Meld.Meld list, pair: Pair.Pair) =
        let meldTiles = 
            melds 
            |> List.collect (fun meld ->
                match Meld.getMeldValue meld with
                | Meld.Sequence(t1, t2, t3) -> [t1; t2; t3]
                | Meld.Triplet(t1, t2, t3) -> [t1; t2; t3])
        let pairTiles = Pair.getPairTiles pair
        meldTiles @ pairTiles

    // タンヤオ（断么九）の判定
    // 全ての牌が中張牌（2-8の数牌）である必要がある
    let checkTanyao (winningHand: Hand.WinningHand) : Option<Yaku> =
        let decompositions = Hand.getDecompositions winningHand
        let (melds, pair) = List.head decompositions  // 任意の分解パターン（牌は同じ）
        let allTiles = getAllTilesFromDecomposition (melds, pair)
        
        if allTiles |> List.forall isSimple then Some Tanyao else None

    // 役の名前を取得
    let getName = function
        | Tanyao -> "断么九"

    // 役の翻数を取得
    let getHan = function
        | Tanyao -> 1

    // 役の英語名を取得
    let getEnglishName = function
        | Tanyao -> "All Simples"