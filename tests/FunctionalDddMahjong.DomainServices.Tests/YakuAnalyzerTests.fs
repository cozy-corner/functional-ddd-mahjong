namespace FunctionalDddMahjong.DomainServices.Tests

open Xunit
open FunctionalDddMahjong.Domain
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.Domain.Hand
open FunctionalDddMahjong.Domain.Yaku
open FunctionalDddMahjong.DomainServices
open FunctionalDddMahjong.DomainServices.YakuAnalyzer

module YakuAnalyzerTests =

    // 期待する役のリスト型
    type ExpectedYaku = Yaku.Yaku list

    // 役名から役への変換ヘルパー
    let private yakuFromString yakuName =
        match yakuName with
        | "Tanyao" -> Yaku.Tanyao
        | "Pinfu" -> Yaku.Pinfu
        | "Toitoi" -> Yaku.Toitoi
        | "Honitsu" -> Yaku.Honitsu
        | "Chinitsu" -> Yaku.Chinitsu
        | "Iipeikou" -> Yaku.Iipeikou
        | _ -> failwith $"Unknown yaku: {yakuName}"

    // テストヘルパー関数
    let private createTestHand (tileStrings: string list) =
        let tiles =
            tileStrings
            |> List.map (fun s ->
                match tryParseFromString s with
                | Ok tile -> tile
                | Error msg -> failwith $"Failed to create tile from {s}: {msg}")

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

    [<Theory>]
    [<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,1p,2p,3p,E,S")>] // 14牌だが和了していない手牌
    let ``analyzeYaku should return error for non-winning hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand = createTestHand tileStrings

        let result = YakuAnalyzer.analyzeYaku hand

        match result with
        | Error(YakuError.InvalidHandState msg) -> Assert.Contains("和了していない", msg)
        | _ -> Assert.True(false, "Expected error for non-winning hand")

    [<Theory>]
    [<InlineData("2m,3m,4m,5m,6m,7m,2p,3p,4p,3s,3s,3s,5s,5s", "Tanyao", 1)>] // タンヤオのみ
    [<InlineData("1m,2m,3m,4m,5m,6m,1p,2p,3p,1s,2s,3s,1m,1m", "Pinfu", 1)>] // ピンフのみ
    [<InlineData("1m,1m,1m,2p,2p,2p,3s,3s,3s,4m,4m,4m,E,E", "Toitoi", 2)>] // トイトイのみ
    // TODO: Phase 3.1 PR5で対応 - 相互排他性の実装
    // [<InlineData("2m,2m,2m,3p,3p,3p,4s,4s,4s,5m,5m,5m,6p,6p", "Toitoi", 2)>] // トイトイ+タンヤオの相互排他性テスト
    let ``analyzeYaku should detect single yaku`` (tileString: string, expectedYaku: string, expectedHan: int) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand = createTestHand tileStrings
        let result = YakuAnalyzer.analyzeYaku hand

        match result with
        | Ok analysisResult ->
            Assert.Equal(1, analysisResult.Yaku.Length)
            Assert.Equal(expectedHan, analysisResult.TotalHan)

            let yakuName = yakuFromString expectedYaku

            Assert.Contains(yakuName, analysisResult.Yaku)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Theory>]
    [<InlineData("2m,3m,4m,5m,6m,7m,2p,3p,4p,2s,3s,4s,2s,2s", "Tanyao,Pinfu", 2)>] // タンヤオ+ピンフ
    [<InlineData("1m,1m,1m,2m,2m,3m,3m,3m,E,E,E,S,S,S", "Honitsu,Toitoi", 5)>] // ホンイツ+トイトイ
    [<InlineData("1m,1m,1m,2m,2m,3m,3m,3m,4m,4m,5m,5m,6m,6m", "Chinitsu,Iipeikou", 7)>] // チンイツ+一盃口
    let ``analyzeYaku should detect multiple yaku`` (tileString: string, expectedYakuString: string, expectedHan: int) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand = createTestHand tileStrings
        let result = YakuAnalyzer.analyzeYaku hand

        match result with
        | Ok analysisResult ->
            Assert.Equal(expectedHan, analysisResult.TotalHan)

            let expectedYakuList =
                expectedYakuString.Split(',')
                |> Array.map yakuFromString
                |> Array.toList

            Assert.Equal(expectedYakuList.Length, analysisResult.Yaku.Length)

            for expectedYaku in expectedYakuList do
                Assert.Contains(expectedYaku, analysisResult.Yaku)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Theory>]
    [<InlineData("1m,2m,3m,1p,2p,3p,1s,2s,3s,1m,1m,1m,E,E")>] // 役なし
    let ``analyzeYaku should detect no yaku`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand = createTestHand tileStrings

        let result = YakuAnalyzer.analyzeYaku hand

        match result with
        | Ok analysisResult ->
            Assert.Empty(analysisResult.Yaku)
            Assert.Equal(0, analysisResult.TotalHan)
            Assert.True(YakuAnalyzer.hasNoYaku analysisResult)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Theory>]
    [<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,3m,R,R,R,G,G", "Honitsu,Toitoi", 5)>] // 111222333mRRRGG - ホンイツ+トイトイ（一盃口は相互排他で除外）
    [<InlineData("2m,2m,2m,3m,3m,3m,4m,4m,4m,5p,5p,5p,6s,6s", "Tanyao,Toitoi", 3)>] // タンヤオ+トイトイ（一盃口は相互排他で除外）
    let ``analyzeYaku should apply high-scoring method for mutually exclusive yaku``
        (tileString: string, expectedYaku: string, expectedHan: int)
        =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand = createTestHand tileStrings
        let result = YakuAnalyzer.analyzeYaku hand

        match result with
        | Ok analysisResult ->
            let expectedYakuList =
                expectedYaku.Split(',')
                |> Array.map yakuFromString
                |> Array.toList

            Assert.Equal<Set<Yaku.Yaku>>(Set.ofList expectedYakuList, Set.ofList analysisResult.Yaku)
            Assert.Equal(expectedHan, analysisResult.TotalHan)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``hasNoYaku should detect empty yaku list`` () =
        let noYakuResult =
            { Yaku = []; TotalHan = 0 }

        let withYakuResult =
            { Yaku = [ Yaku.Tanyao ]; TotalHan = 1 }

        Assert.True(YakuAnalyzer.hasNoYaku noYakuResult)
        Assert.False(YakuAnalyzer.hasNoYaku withYakuResult)
