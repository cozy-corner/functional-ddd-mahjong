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
        let result = tryDecomposeAll waitingHand
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
[<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,3m,4m,4m,4m,5m,5m", "2m:111m,234m,345m,345m|5m:111m,222m,333m,444m|5m:111m,234m,234m,234m|5m:123m,123m,123m,444m")>]
// 雀頭候補が複数ある手牌 (複数分解パターン)
[<InlineData("1m,1m,2m,2m,3m,3m,4m,4m,5m,5m,6m,6m,7m,7m", "1m:234m,234m,567m,567m|4m:123m,123m,567m,567m|7m:123m,123m,456m,456m")>]
// 同じ牌群での刻子と順子の選択が可能なパターン (複数分解)
[<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,3m,4p,5p,6p,7s,7s", "7s:111m,222m,333m,456p|7s:123m,123m,123m,456p")>]
let ``tryDecomposeAll returns correct decomposition patterns`` (tileString: string, expectedPatternsString: string) =
    let tileStrings = tileString.Split(',') |> Array.toList
    let hand = createReadyHand (createTiles tileStrings)
    let result = tryDecomposeAll hand
    
    let actualPatterns = 
        result
        |> List.map (fun (melds, pair) ->
            let pairStr = getPairTiles pair |> List.head |> toShortString
            let meldStrs = melds |> List.map meldToShortString |> List.sort
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

        let result = tryDecomposeAll hand
        Assert.Empty(result))
