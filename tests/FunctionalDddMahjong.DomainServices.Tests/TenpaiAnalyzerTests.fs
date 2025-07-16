module FunctionalDddMahjong.DomainServices.Tests.TenpaiAnalyzerTests

open Xunit
open FunctionalDddMahjong.Domain
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.DomainServices.TenpaiAnalyzer

// テスト用ヘルパー関数
let private parseTiles (tilesStr: string) =
    tilesStr.Split(',')
    |> Array.toList
    |> List.map (fun s ->
        match Tile.tryParseFromString s with
        | Ok tile -> tile
        | Error err -> failwith (sprintf "Failed to parse tile '%s': %A" s err))

// テンパイ判定のパラメータ化テスト
[<Theory>]
[<InlineData("1m,1m,1m,2p,2p,2p,3s,3s,3s,E,E,E,E", true, "単騎待ち")>]
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,1m,1m,2m,3m", true, "両面待ち")>]
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,1m,1m,1m,3m", true, "嵌張待ち")>]
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,1m,1m,8m,9m", true, "辺張待ち89")>]
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,1p,1p,1m,2m", true, "辺張待ち12")>]
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,1m,1m,2p,2p", true, "双碰待ち")>]
[<InlineData("1m,3m,5m,7m,9m,1p,3p,5p,7p,9p,1s,E,S", false, "バラバラ")>]
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,1m,3m,5m,E,S", false, "面子不足")>]
[<InlineData("1m,1m,2m,2m,3m,3m,4m,4m,5m,5m,E,E,S", false, "七対子形")>]
[<InlineData("1m,9m,1p,9p,1s,9s,E,S,W,N,R,G,H", false, "国士形")>]
let ``isTenpai correctly identifies tenpai patterns`` (tilesStr: string, expected: bool, description: string) =
    let tiles = parseTiles tilesStr
    let result = isTenpai tiles
    Assert.Equal(expected, result)

// 待ち牌取得のパラメータ化テスト
[<Theory>]
[<InlineData("1m,1m,1m,2p,2p,2p,3s,3s,3s,E,E,E,E", "E")>] // 単騎待ち
[<InlineData("E,E,E,S,S,S,W,W,W,N,N,2m,3m", "1m,4m")>] // 両面待ち
[<InlineData("E,E,E,S,S,S,W,W,W,N,N,1m,3m", "2m")>] // 嵌張待ち
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,1m,1m,8m,9m", "7m")>] // 辺張待ち
[<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,1p,1p,1m,2m", "3m")>] // 辺張待ち
[<InlineData("E,E,E,S,S,S,W,W,W,1m,1m,2m,2m", "1m,2m")>] // 双碰待ち
[<InlineData("1m,3m,5m,7m,9m,1p,3p,5p,7p,9p,1s,E,S", "")>] // ノーテン
let ``getWaitingTiles returns correct tiles`` (tilesStr: string, expectedStr: string) =
    let tiles = parseTiles tilesStr
    let waitingTiles = getWaitingTiles tiles

    let expectedTiles =
        if expectedStr = "" then
            []
        else
            expectedStr.Split(',')
            |> Array.toList
            |> List.map (fun s ->
                match Tile.tryParseFromString s with
                | Ok tile -> tile
                | Error err -> failwith (sprintf "Failed to parse tile '%s': %A" s err))

    Assert.Equal<Tile list>(expectedTiles, waitingTiles)

// 複合パターンの待ち牌テスト
[<Theory>]
[<InlineData("E,E,E,S,S,S,W,W,W,1m,1m,1m,2m", "2m,3m")>] // 1112形: 刻子+単騎 or 雀頭+辺張
[<InlineData("E,E,E,S,S,S,W,W,W,1m,1m,1m,3m", "2m,3m")>] // 1113形: 刻子+単騎 or 雀頭+嵌張
[<InlineData("E,E,E,S,S,S,1m,1m,1m,2m,3m,4m,5m", "2m,3m,5m,6m")>] // 1112345形: 2,5,3,6待ち
[<InlineData("1m,1m,1m,2m,3m,4m,5m,6m,7m,8m,9m,9m,9m", "1m,2m,3m,4m,5m,6m,7m,8m,9m")>] // 純正九蓮宝燈: 全待ち
let ``getWaitingTiles handles complex decomposition patterns`` (tilesStr: string, expectedStr: string) =
    let tiles = parseTiles tilesStr
    let waitingTiles = getWaitingTiles tiles

    let expectedTiles =
        if expectedStr = "" then
            []
        else
            expectedStr.Split(',')
            |> Array.toList
            |> List.map (fun s ->
                match Tile.tryParseFromString s with
                | Ok tile -> tile
                | Error err -> failwith (sprintf "Failed to parse tile '%s': %A" s err))

    Assert.Equal<Tile list>(expectedTiles, waitingTiles)

// 複雑なテンパイパターンのテスト
[<Theory>]
[<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,4p,5p,6p,E,E", true)>] // 複数の待ちパターン
[<InlineData("1m,2m,3m,1m,2m,3m,1m,2m,3m,1p,1p,4s,5s", true)>] // 一盃口形
[<InlineData("1m,1m,2m,2m,3m,3m,7p,7p,8p,8p,9p,9p,5s", true)>] // 二盃口形
[<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,1p,1p,E,E", true)>] // 清一色形
[<InlineData("E,E,E,S,S,S,W,W,W,N,N,1m,2m", true)>] // 字牌多め
let ``isTenpai handles complex patterns`` (tilesStr: string, expected: bool) =
    let tiles = parseTiles tilesStr
    let result = isTenpai tiles
    Assert.Equal(expected, result)

// エッジケースのテスト
[<Theory>]
[<InlineData("1m,1m,1m,1m,2m,2m,2m,2m,3m,3m,3m,3m,4m", true)>] // 同じ牌4枚使用
[<InlineData("1p,2p,3p,4p,5p,6p,7p,8p,9p,1s,2s,3s,4s", true)>] // 連続する牌
[<InlineData("E,E,E,S,S,S,W,W,W,N,N,N,R", true)>] // 字牌4面子単騎待ち
let ``isTenpai handles edge cases`` (tilesStr: string, expected: bool) =
    let tiles = parseTiles tilesStr
    let result = isTenpai tiles
    Assert.Equal(expected, result)

// 待ち牌の数のテスト
[<Theory>]
[<InlineData("1m,1m,1m,2p,2p,2p,3s,3s,3s,E,E,E,E", 1)>] // 単騎待ち: 1種類
[<InlineData("E,E,E,S,S,S,W,W,W,N,N,2m,3m", 2)>] // 両面待ち: 2種類
[<InlineData("E,E,E,S,S,S,W,W,W,N,N,1m,3m", 1)>] // 嵌張待ち: 1種類
[<InlineData("E,E,E,S,S,S,W,W,W,1m,1m,2m,2m", 2)>] // 双碰待ち: 2種類
[<InlineData("1m,3m,5m,7m,9m,1p,3p,5p,7p,9p,1s,E,S", 0)>] // ノーテン: 0種類
let ``getWaitingTiles returns correct number of waiting tiles`` (tilesStr: string, expectedCount: int) =
    let tiles = parseTiles tilesStr
    let waitingTiles = getWaitingTiles tiles
    Assert.Equal(expectedCount, List.length waitingTiles)

// 不正な入力のテスト
[<Fact>]
let ``isTenpai returns false for less than 13 tiles`` () =
    let tiles = parseTiles "1m,2m,3m,4p,5p,6p"
    let result = isTenpai tiles
    Assert.False(result)

[<Fact>]
let ``isTenpai returns false for more than 13 tiles`` () =
    let tiles =
        parseTiles "1m,2m,3m,4p,5p,6p,7s,8s,9s,E,E,E,E,E,E"

    let result = isTenpai tiles
    Assert.False(result)

[<Fact>]
let ``getWaitingTiles returns empty for invalid tile count`` () =
    let tiles = parseTiles "1m,2m,3m"
    let waitingTiles = getWaitingTiles tiles
    Assert.Empty(waitingTiles)
