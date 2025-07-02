module FunctionalDddMahjong.Domain.Tests.HandDecompositionTests

open Xunit
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.Domain.Hand
open FunctionalDddMahjong.Domain.Meld
open FunctionalDddMahjong.Domain.Pair

// テスト用のヘルパー関数: 文字列のリストから牌を作成
let private createTiles tileStrings =
    tileStrings
    |> List.choose (fun str ->
        match tryParseFromString str with
        | Ok tile -> Some tile
        | Error _ -> None)

// テスト用のヘルパー関数: 14牌の手牌を作成
let private createReadyHand tiles =
    match tiles with
    | [] -> failwith "Empty tile list"
    | _ when List.length tiles <> 14 -> failwith $"Expected 14 tiles, got {List.length tiles}"
    | _ ->
        // 最初の13牌で手牌を作り、14牌目をツモる
        let first13 = tiles |> List.take 13
        let lastTile = tiles |> List.item 13

        match tryCreateFromDeal first13 with
        | Ok waitingHand -> draw lastTile waitingHand
        | Error e -> failwith $"Failed to create hand: {e}"

[<Fact>]
let ``tryDecompose returns None for 13-tile waiting hand`` () =
    // 13牌の手牌
    let tiles =
        createTiles
            [ "1m"
              "2m"
              "3m"
              "4m"
              "5m"
              "6m"
              "7m"
              "8m"
              "9m"
              "1p"
              "2p"
              "3p"
              "4p" ]

    match tryCreateFromDeal tiles with
    | Ok waitingHand ->
        let result = tryDecompose waitingHand
        Assert.True(Option.isNone result)
    | Error e -> failwith $"Test setup failed: {e}"

[<Fact>]
let ``tryDecompose finds simple winning hand with sequences and triplet`` () =
    // 簡単な和了形: 123m 456m 789m 111p 22s (雀頭が一意に決まる)
    let tiles =
        createTiles
            [ "1m"
              "2m"
              "3m"
              "4m"
              "5m"
              "6m"
              "7m"
              "8m"
              "9m"
              "1p"
              "1p"
              "1p"
              "2s"
              "2s" ]

    let hand = createReadyHand tiles

    let result = tryDecompose hand

    // まず結果があるかどうか確認
    Assert.True(Option.isSome result, "Expected Some but got None")

    match result with
    | Some(melds, pair) ->
        // 4面子あることを確認
        Assert.Equal(4, List.length melds)

        // 雀頭が2sであることを確認（これしか選択肢がない）
        let pairTiles = getPairTiles pair
        Assert.Equal(2, List.length pairTiles)

        let actualPairString =
            pairTiles
            |> List.map toString
            |> String.concat ", "

        Assert.Equal("2索, 2索", actualPairString)

    | None ->
        // デバッグ: 手牌の内容を出力
        let handTiles =
            getTiles hand
            |> List.map toString
            |> String.concat ", "

        failwith $"Expected successful decomposition. Hand tiles: [{handTiles}]"

[<Fact>]
let ``tryDecompose finds winning hand with all triplets`` () =
    // 刻子のみの和了形: 111m 222m 333m 444m 55m
    let tiles =
        createTiles
            [ "1m"
              "1m"
              "1m"
              "2m"
              "2m"
              "2m"
              "3m"
              "3m"
              "3m"
              "4m"
              "4m"
              "4m"
              "5m"
              "5m" ]

    let hand = createReadyHand tiles

    match tryDecompose hand with
    | Some(melds, pair) ->
        // 4面子あることを確認
        Assert.Equal(4, List.length melds)

        // 全て刻子であることを確認
        Assert.True(
            melds
            |> List.forall (fun meld ->
                match getMeldValue meld with
                | Triplet _ -> true
                | _ -> false)
        )

        // 雀頭が5mであることを確認
        let pairTiles = getPairTiles pair
        Assert.Equal(2, List.length pairTiles)

        Assert.True(
            pairTiles
            |> List.forall (fun t -> toString t = "5萬")
        )

    | None -> failwith "Expected successful decomposition"

[<Fact>]
let ``tryDecompose returns None for non-winning hand`` () =
    // 和了形ではない手牌: 面子が作れない
    let tiles =
        createTiles
            [ "1m"
              "3m"
              "5m"
              "7m"
              "9m"
              "1p"
              "3p"
              "5p"
              "7p"
              "9p"
              "1s"
              "3s"
              "5s"
              "7s" ]

    let hand = createReadyHand tiles

    let result = tryDecompose hand
    Assert.True(Option.isNone result)

[<Fact>]
let ``tryDecompose handles mixed suits correctly`` () =
    // 混合型の和了形: 123m 456p 789s EEE NN
    let tiles =
        createTiles
            [ "1m"
              "2m"
              "3m"
              "4p"
              "5p"
              "6p"
              "7s"
              "8s"
              "9s"
              "E"
              "E"
              "E"
              "N"
              "N" ]

    let hand = createReadyHand tiles

    match tryDecompose hand with
    | Some(melds, pair) ->
        // 4面子あることを確認
        Assert.Equal(4, List.length melds)

        // 雀頭が北であることを確認
        let pairTiles = getPairTiles pair
        Assert.Equal(2, List.length pairTiles)

        Assert.True(
            pairTiles
            |> List.forall (fun t -> toString t = "北")
        )

    | None -> failwith "Expected successful decomposition"
