module FunctionalDddMahjong.DomainServices.Tests.HandDecompositionTests

open Xunit
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.Domain.Hand
open FunctionalDddMahjong.Domain
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


// tryDecomposeAll のテスト

[<Fact>]
let ``tryDecomposeAll returns empty list for 13-tile waiting hand`` () =
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
        let result =
            Hand.tryDecomposeAll waitingHand

        Assert.Empty(result)
    | Error e -> failwith $"Test setup failed: {e}"

[<Theory>]
// 基本的な和了形: 順子3つ + 刻子1つ + 雀頭
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,E,E,E,W,W", "W:123m,456p,789s,EEE")>]
// 一盃口: 同じ順子2つを含む和了形
[<InlineData("1m,1m,2m,2m,3m,3m,4p,5p,6p,7s,8s,9s,E,E", "E:123m,123m,456p,789s")>]
// 順子4つの和了形
[<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,1p,1p,1p,2s,2s", "2s:111p,123m,456m,789m")>]
// 刻子と順子の両方の分解が可能なパターン (複数分解)
[<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,3m,4m,4m,4m,5m,5m",
             "2m:111m,234m,345m,345m|5m:111m,222m,333m,444m|5m:111m,234m,234m,234m|5m:123m,123m,123m,444m")>]
// 雀頭候補が複数ある手牌 (複数分解パターン)
[<InlineData("1m,1m,2m,2m,3m,3m,4m,4m,5m,5m,6m,6m,7m,7m",
             "1m:234m,234m,567m,567m|4m:123m,123m,567m,567m|7m:123m,123m,456m,456m")>]
// 同じ牌群での刻子と順子の選択が可能なパターン (複数分解)
[<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,3m,4p,5p,6p,7s,7s", "7s:111m,222m,333m,456p|7s:123m,123m,123m,456p")>]
let ``tryDecomposeAll returns correct decomposition patterns`` (tileString: string, expectedPatternsString: string) =
    let tileStrings =
        tileString.Split(',') |> Array.toList

    let hand =
        createReadyHand (createTiles tileStrings)

    let result = Hand.tryDecomposeAll hand

    let actualPatterns =
        result
        |> List.map (fun (melds, pair) ->
            let pairStr =
                Pair.getPairTiles pair
                |> List.head
                |> Tile.toShortString

            let meldStrs =
                melds
                |> List.map Meld.meldToShortString
                |> List.sort

            let meldsStr = String.concat "," meldStrs
            $"{pairStr}:{meldsStr}")
        |> List.sort

    let expectedPatterns =
        expectedPatternsString.Split('|')
        |> Array.toList
        |> List.sort

    Assert.Equal<string list>(expectedPatterns, actualPatterns)



[<Fact>]
let ``tryDecomposeAll returns empty for invalid hands`` () =
    [
      // 和了形ではない手牌
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
        "7s" ] ]
    |> List.iter (fun tileStrings ->
        let hand =
            createReadyHand (createTiles tileStrings)

        let result = Hand.tryDecomposeAll hand
        Assert.Empty(result))


// isWinningHand のテスト

[<Theory>]
// 基本的な和了形のパターン
[<InlineData("1m,2m,3m,E,E,E,S,S,S,W,W,W,N,N")>] // 順子1つ + 刻子3つ + 雀頭
[<InlineData("1m,1m,1m,E,E,E,S,S,S,W,W,W,N,N")>] // 刻子4つ + 雀頭
[<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,E,E,E,S,S")>] // 順子3つ + 刻子1つ + 雀頭
[<InlineData("1m,1m,2m,2m,3m,3m,E,E,E,S,S,S,W,W")>] // 一盃口（同じ順子2つ）を含む和了形
// 清一色（同じ種類の牌のみ）
[<InlineData("1m,1m,1m,2m,3m,4m,5m,6m,7m,8m,9m,9m,9m,9m")>] // 萬子清一色
[<InlineData("1p,2p,3p,2p,3p,4p,3p,4p,5p,7p,8p,9p,5p,5p")>] // 筒子清一色
[<InlineData("1s,1s,2s,2s,3s,3s,4s,5s,6s,7s,8s,9s,9s,9s")>] // 索子清一色
// 字牌のみの和了形
[<InlineData("E,E,E,S,S,S,W,W,W,N,N,H,H,H")>] // 字牌のみ（字一色）
// 複雑なパターン（複数の分解が可能）
[<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,3m,4m,4m,4m,5m,5m")>] // 同じ牌群で刻子/順子の選択可能
[<InlineData("1m,1m,2m,2m,3m,3m,4m,4m,5m,5m,6m,6m,7m,7m")>] // 雀頭候補が複数ある
let ``isWinningHand returns true for valid winning hands`` (tileString: string) =
    let tileStrings =
        tileString.Split(',') |> Array.toList

    let hand =
        createReadyHand (createTiles tileStrings)

    Assert.True(Hand.isWinningHand hand)

[<Theory>]
// ノーテン手（和了にならない手牌）のパターン
[<InlineData("1m,3m,5m,7m,9m,1p,3p,5p,7p,9p,E,S,W,N")>] // 全て孤立牌
[<InlineData("1m,1m,3m,3m,5m,5m,7m,7m,9m,9m,E,S,W,N")>] // 対子のみで面子なし
[<InlineData("1m,2m,4m,5m,E,E,E,S,S,S,W,W,N,H")>] // 面子になりかけ（1枚足りない）
[<InlineData("1m,2m,3m,E,E,E,S,S,S,W,N,H,G,R")>] // 3面子しか作れない（雀頭候補なし）
[<InlineData("1m,1m,1m,1m,E,E,E,E,S,S,S,S,W,W")>] // 同じ牌が4枚（槓子）を含む
[<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,E,S,W,N,H")>] // 雀頭がない（全て異なる字牌）
// 惜しいノーテン（あと1枚で和了）
[<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,E,E,E,S,W")>] // W→Sなら和了（雀頭不足）
[<InlineData("1m,1m,1m,2m,3m,5m,E,E,E,S,S,S,W,W")>] // 5m→4mなら和了（順子不完全）
let ``isWinningHand returns false for non-winning hands`` (tileString: string) =
    let tileStrings =
        tileString.Split(',') |> Array.toList

    let hand =
        createReadyHand (createTiles tileStrings)

    Assert.False(Hand.isWinningHand hand)

[<Fact>]
let ``isWinningHand returns false for 13-tile waiting hand`` () =
    // 13牌の手牌（どんな組み合わせでも和了にならない）
    let tiles =
        createTiles
            [ "1m"
              "2m"
              "3m"
              "E"
              "E"
              "E"
              "S"
              "S"
              "S"
              "W"
              "W"
              "W"
              "N" ]

    match tryCreateFromDeal tiles with
    | Ok waitingHand -> Assert.False(Hand.isWinningHand waitingHand)
    | Error e -> failwith $"Test setup failed: {e}"
