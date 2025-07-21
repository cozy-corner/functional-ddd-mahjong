module FunctionalDddMahjong.Api.Tests.CheckWinningHandHandlerTests

open Xunit
open FunctionalDddMahjong.Api
open FunctionalDddMahjong.Api.CheckWinningHandHandler
open FunctionalDddMahjong.Domain

/// Test helpers
module TestData =
    // Valid 14-tile winning hand: 123m 456p 789s EEE WW
    let validWinningHand =
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

    // Valid 14-tile hand with all tile types
    let validMixedHand =
        [ "1m"
          "2m"
          "3m"
          "1p"
          "2p"
          "3p"
          "1s"
          "2s"
          "3s"
          "E"
          "S"
          "W"
          "N"
          "N" ]

    // 13 tiles (too few)
    let thirteenTiles =
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

    // 15 tiles (too many)
    let fifteenTiles =
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
          "W"
          "W" ]

/// Tests for parseTile function
module ParseTileTests =

    [<Theory>]
    [<InlineData("1m")>]
    [<InlineData("9m")>]
    [<InlineData("1p")>]
    [<InlineData("9p")>]
    [<InlineData("1s")>]
    [<InlineData("9s")>]
    [<InlineData("E")>]
    [<InlineData("S")>]
    [<InlineData("W")>]
    [<InlineData("N")>]
    [<InlineData("G")>]
    [<InlineData("R")>]
    [<InlineData("H")>]
    let ``parseTile should parse valid tile strings`` (tileStr: string) =
        let result = parseTile tileStr

        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Theory>]
    [<InlineData("10m", "Invalid tile: 10m")>]
    [<InlineData("0p", "Invalid tile: 0p")>]
    [<InlineData("X", "Invalid tile: X")>]
    [<InlineData("", "Invalid tile: ")>]
    [<InlineData("1", "Invalid tile: 1")>]
    [<InlineData("mm", "Invalid tile: mm")>]
    let ``parseTile should return error for invalid tile strings`` (tileStr: string, expectedError: string) =
        let result = parseTile tileStr

        match result with
        | Ok _ -> Assert.True(false, $"Expected error but got success")
        | Error msg -> Assert.Equal(expectedError, msg)

/// Tests for parseAndValidateRequest function
module ParseAndValidateRequestTests =

    [<Fact>]
    let ``parseAndValidateRequest should accept valid 14-tile winning hand`` () =
        let request =
            { tiles = TestData.validWinningHand }

        let result = parseAndValidateRequest request

        match result with
        | Ok hand ->
            let tiles = Hand.getTiles hand
            Assert.Equal(14, List.length tiles)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``parseAndValidateRequest should accept valid mixed hand with all tile types`` () =
        let request =
            { tiles = TestData.validMixedHand }

        let result = parseAndValidateRequest request

        match result with
        | Ok hand ->
            let tiles = Hand.getTiles hand
            Assert.Equal(14, List.length tiles)
        | Error msg -> Assert.True(false, $"Expected success but got error: {msg}")

    [<Fact>]
    let ``parseAndValidateRequest should reject 13 tiles`` () =
        let request =
            { tiles = TestData.thirteenTiles }

        let result = parseAndValidateRequest request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Equal("Hand must have exactly 14 tiles, but got 13", msg)

    [<Fact>]
    let ``parseAndValidateRequest should reject 15 tiles`` () =
        let request =
            { tiles = TestData.fifteenTiles }

        let result = parseAndValidateRequest request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Equal("Hand must have exactly 14 tiles, but got 15", msg)

    [<Fact>]
    let ``parseAndValidateRequest should reject 0 tiles`` () =
        let request = { tiles = [] }
        let result = parseAndValidateRequest request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Equal("Hand must have exactly 14 tiles, but got 0", msg)

    [<Fact>]
    let ``parseAndValidateRequest should reject hand with invalid tile (fail-fast)`` () =
        let tilesWithInvalid =
            [ "1m"
              "2m"
              "3m"
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

        let request = { tiles = tilesWithInvalid }
        let result = parseAndValidateRequest request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Equal("Invalid tile: 10m", msg)

    [<Fact>]
    let ``parseAndValidateRequest should reject hand with duplicate limit violation`` () =
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
              "8s"
              "9s" ]

        let request =
            { tiles = tilesWithDuplicates }

        let result = parseAndValidateRequest request

        match result with
        | Ok _ -> Assert.True(false, "Expected error but got success")
        | Error msg -> Assert.Contains("同じ牌は4枚まで", msg)
