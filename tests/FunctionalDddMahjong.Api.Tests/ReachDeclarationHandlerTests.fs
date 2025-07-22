module FunctionalDddMahjong.Api.Tests.ReachDeclarationHandlerTests

open Xunit
open FunctionalDddMahjong.Api
open FunctionalDddMahjong.Api.ReachDeclarationHandler

/// Test helpers
module TestData =
    // Valid 13-tile tenpai hand: 123m 456p 789s EEE W (waiting for another W)
    let validTenpaiHand =
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
          "W" ]

    // Valid 13-tile non-tenpai hand
    let nonTenpaiHand =
        [ "1m"
          "3m"
          "5m"
          "2p"
          "4p"
          "6p"
          "1s"
          "3s"
          "5s"
          "E"
          "S"
          "W"
          "N" ]

    // 14 tiles (too many)
    let fourteenTiles =
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
          "W"
          "W" ]

    // 12 tiles (too few)
    let twelveTiles =
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
          "E" ]

/// Tests for main endpoint function
module HandleDeclareReachTests =

    [<Fact>]
    let ``handleDeclareReach should succeed for valid tenpai hand with sufficient score`` () =
        let request =
            { tiles = TestData.validTenpaiHand
              score = 25000
              turn = 10
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok response ->
            Assert.True(response.success)
            Assert.Equal(Some "Reach", response.reachType)
            Assert.True(response.error.IsNone)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``handleDeclareReach should return DoubleReach for turn 1`` () =
        let request =
            { tiles = TestData.validTenpaiHand
              score = 25000
              turn = 1
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok response ->
            Assert.True(response.success)
            Assert.Equal(Some "DoubleReach", response.reachType)
            Assert.True(response.error.IsNone)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``handleDeclareReach should fail for non-tenpai hand`` () =
        let request =
            { tiles = TestData.nonTenpaiHand
              score = 25000
              turn = 10
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok response ->
            Assert.False(response.success)
            Assert.True(response.reachType.IsNone)
            Assert.Equal(Some "手牌が聴牌していません", response.error)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``handleDeclareReach should fail for insufficient score`` () =
        let request =
            { tiles = TestData.validTenpaiHand
              score = 500
              turn = 10
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok response ->
            Assert.False(response.success)
            Assert.True(response.reachType.IsNone)
            Assert.Equal(Some "点数が不足しています（1000点必要）", response.error)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``handleDeclareReach should fail after turn 15`` () =
        let request =
            { tiles = TestData.validTenpaiHand
              score = 25000
              turn = 16
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok response ->
            Assert.False(response.success)
            Assert.True(response.reachType.IsNone)
            Assert.Equal(Some "ゲーム終盤のためリーチできません（15巡目まで）", response.error)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``handleDeclareReach should fail if already reached`` () =
        let request =
            { tiles = TestData.validTenpaiHand
              score = 25000
              turn = 10
              hasReached = true }

        let result = handleDeclareReach request

        match result with
        | Ok response ->
            Assert.False(response.success)
            Assert.True(response.reachType.IsNone)
            Assert.Equal(Some "すでにリーチしています", response.error)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``handleDeclareReach should reject 14 tiles`` () =
        let request =
            { tiles = TestData.fourteenTiles
              score = 25000
              turn = 10
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Equal("Reach declaration requires exactly 13 tiles, but got 14", msg)

    [<Fact>]
    let ``handleDeclareReach should reject 12 tiles`` () =
        let request =
            { tiles = TestData.twelveTiles
              score = 25000
              turn = 10
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Equal("Reach declaration requires exactly 13 tiles, but got 12", msg)

    [<Fact>]
    let ``handleDeclareReach should reject hand with duplicate limit violation`` () =
        // 5 East winds (more than 4 allowed)
        let tilesWithDuplicates =
            [ "E"
              "E"
              "E"
              "E"
              "E"
              "1m"
              "2m"
              "3m"
              "4p"
              "5p"
              "6p"
              "7s"
              "8s" ]

        let request =
            { tiles = tilesWithDuplicates
              score = 25000
              turn = 10
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Contains("同じ牌は4枚まで", msg)

    [<Fact>]
    let ``handleDeclareReach should fail for invalid tile in request`` () =
        let request =
            { tiles =
                [ "1m"
                  "2m"
                  "10m"
                  "4p"
                  "5p"
                  "6p"
                  "7s"
                  "8s"
                  "9s"
                  "E"
                  "E"
                  "E"
                  "W" ]
              score = 25000
              turn = 10
              hasReached = false }

        let result = handleDeclareReach request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Equal("Invalid tile: 10m", msg)
