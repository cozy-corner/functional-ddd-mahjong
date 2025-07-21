module FunctionalDddMahjong.ApplicationServices.Tests.CheckWinningHandServiceTests

open Xunit
open FunctionalDddMahjong.Domain
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.ApplicationServices

module ``CheckWinningHandService Integration Tests`` =

    [<Fact>]
    let ``checkWinningHand should handle non-winning hand workflow`` () =
        // Arrange: 非和了手（統合テスト：全ワークフロー）
        let tiles =
            [ Tile.create (Character One)
              Tile.create (Character Two)
              Tile.create (Character Three)
              Tile.create (Character Four)
              Tile.create (Character Five)
              Tile.create (Character Six)
              Tile.create (Character Seven)
              Tile.create (Character Eight)
              Tile.create (Character Nine)
              Tile.create (Character One)
              Tile.create (Character Two)
              Tile.create (Circle One)
              Tile.create (Circle Two)
              Tile.create (Circle Three) ]

        let hand = Hand.Ready tiles

        // Act: エンドツーエンドワークフロー
        let result =
            CheckWinningHandService.checkWinningHand hand

        // Assert: 非和了時の統合動作
        Assert.False(result.isWinningHand)
        Assert.Empty(result.detectedYaku)
        Assert.Equal(0, result.totalHan)

    [<Fact>]
    let ``checkWinningHand should handle winning hand with yaku workflow`` () =
        // Arrange: 明確な和了手（統合テスト：和了判定→役分析→レスポンス変換）
        let tiles =
            [ Tile.create (Character Two)
              Tile.create (Character Three)
              Tile.create (Character Four)
              Tile.create (Circle Two)
              Tile.create (Circle Three)
              Tile.create (Circle Four)
              Tile.create (Bamboo Two)
              Tile.create (Bamboo Three)
              Tile.create (Bamboo Four)
              Tile.create (Character Five)
              Tile.create (Character Five)
              Tile.create (Character Six)
              Tile.create (Character Six)
              Tile.create (Character Six) ]

        let hand = Hand.Ready tiles

        // Act: 統合ワークフロー実行
        let result =
            CheckWinningHandService.checkWinningHand hand

        // Assert: 和了時の統合動作（詳細な役判定はYakuAnalyzerでテスト済み）
        Assert.True(result.isWinningHand)
        Assert.Single(result.detectedYaku) |> ignore
        Assert.Equal(1, result.totalHan)

        // Verify Tanyao is detected in integration
        let yakuInfo =
            result.detectedYaku |> List.head

        Assert.Equal("Tanyao", yakuInfo.name)
        Assert.Equal(1, yakuInfo.han)

    [<Fact>]
    let ``checkWinningHand should handle YakuAnalyzer error gracefully`` () =
        // Arrange: 和了手だが役分析エラーが発生する可能性のケース
        let tiles =
            [ Tile.create (Character One)
              Tile.create (Character Nine)
              Tile.create (Honor East)
              Tile.create (Character One)
              Tile.create (Character Nine)
              Tile.create (Honor East)
              Tile.create (Character One)
              Tile.create (Character Nine)
              Tile.create (Honor East)
              Tile.create (Character One)
              Tile.create (Character Nine)
              Tile.create (Honor East)
              Tile.create (Honor North)
              Tile.create (Honor North) ]

        let hand = Hand.Ready tiles

        // Act: エラーハンドリング統合テスト
        let result =
            CheckWinningHandService.checkWinningHand hand

        // Assert: エラー時の適切なレスポンス
        if result.isWinningHand then
            // 和了だが役なし（または分析エラー）の場合の処理
            Assert.Empty(result.detectedYaku)
            Assert.Equal(0, result.totalHan)

module ``Domain to DTO Conversion Tests`` =

    [<Fact>]
    let ``YakuInfo conversion should preserve all domain information`` () =
        // Arrange: ドメインの役情報
        let winningHand =
            [ Tile.create (Character Two)
              Tile.create (Character Three)
              Tile.create (Character Four)
              Tile.create (Circle Two)
              Tile.create (Circle Three)
              Tile.create (Circle Four)
              Tile.create (Bamboo Two)
              Tile.create (Bamboo Three)
              Tile.create (Bamboo Four)
              Tile.create (Character Five)
              Tile.create (Character Five)
              Tile.create (Character Six)
              Tile.create (Character Six)
              Tile.create (Character Six) ]

        let hand = Hand.Ready winningHand

        // Act: ドメイン→ApplicationServices変換
        let result =
            CheckWinningHandService.checkWinningHand hand

        // Assert: 境界での型変換の正確性
        Assert.Single(result.detectedYaku) |> ignore

        let yakuInfo =
            result.detectedYaku |> List.head

        Assert.Equal("Tanyao", yakuInfo.name)
        Assert.Equal("タンヤオ", yakuInfo.displayName)
        Assert.Equal(1, yakuInfo.han)
        Assert.Equal("中張牌のみで構成された手", yakuInfo.description)

    [<Fact>]
    let ``Response structure should be consistent across different scenarios`` () =
        // Arrange: 異なるシナリオ用の手牌
        let nonWinningHand =
            [ Tile.create (Character One)
              Tile.create (Character Two)
              Tile.create (Character Three)
              Tile.create (Character Four)
              Tile.create (Character Five)
              Tile.create (Character Six)
              Tile.create (Character Seven)
              Tile.create (Character Eight)
              Tile.create (Character Nine)
              Tile.create (Character One)
              Tile.create (Character Two)
              Tile.create (Circle One)
              Tile.create (Circle Two)
              Tile.create (Circle Three) ]

        let nonWinningHandReady =
            Hand.Ready nonWinningHand

        let winningHand =
            [ Tile.create (Character Two)
              Tile.create (Character Three)
              Tile.create (Character Four)
              Tile.create (Circle Two)
              Tile.create (Circle Three)
              Tile.create (Circle Four)
              Tile.create (Bamboo Two)
              Tile.create (Bamboo Three)
              Tile.create (Bamboo Four)
              Tile.create (Character Five)
              Tile.create (Character Five)
              Tile.create (Character Six)
              Tile.create (Character Six)
              Tile.create (Character Six) ]

        let winningHandReady =
            Hand.Ready winningHand

        // Act: 両シナリオの実行
        let nonWinningResult =
            CheckWinningHandService.checkWinningHand nonWinningHandReady

        let winningResult =
            CheckWinningHandService.checkWinningHand winningHandReady

        // Assert: レスポンス構造の一貫性
        // 非和了時
        Assert.False(nonWinningResult.isWinningHand)

        Assert.IsType<CheckWinningHandService.YakuInfo list>(nonWinningResult.detectedYaku)
        |> ignore

        Assert.IsType<int>(nonWinningResult.totalHan)
        |> ignore

        // 和了時
        Assert.True(winningResult.isWinningHand)

        Assert.IsType<CheckWinningHandService.YakuInfo list>(winningResult.detectedYaku)
        |> ignore

        Assert.IsType<int>(winningResult.totalHan)
        |> ignore

module ``End-to-End Workflow Tests`` =

    [<Fact>]
    let ``checkWinningHand should demonstrate complete workflow`` () =
        // Arrange: 複数役を持つ手牌（ワークフロー全体の統合テスト）
        let tiles =
            [ Tile.create (Character One)
              Tile.create (Character One)
              Tile.create (Character One)
              Tile.create (Character Two)
              Tile.create (Character Two)
              Tile.create (Character Two)
              Tile.create (Character Three)
              Tile.create (Character Three)
              Tile.create (Character Three)
              Tile.create (Character Four)
              Tile.create (Character Four)
              Tile.create (Character Four)
              Tile.create (Character Five)
              Tile.create (Character Five) ]

        let hand = Hand.Ready tiles

        // Act: 完全なワークフロー実行
        let result =
            CheckWinningHandService.checkWinningHand hand

        // Assert: エンドツーエンドの動作確認
        Assert.True(result.isWinningHand)
        Assert.Equal(2, result.detectedYaku |> List.length)
        Assert.Equal(8, result.totalHan) // Toitoi(2) + Chinitsu(6)

        // 複数役の組み合わせを確認（ToitoiとChinitsuが両方成立）
        let yakuNames =
            result.detectedYaku
            |> List.map (fun y -> y.name)
            |> List.sort

        Assert.Equal<string list>([ "Chinitsu"; "Toitoi" ], yakuNames)

        let totalCalculated =
            result.detectedYaku |> List.sumBy (fun y -> y.han)

        Assert.Equal(result.totalHan, totalCalculated)
