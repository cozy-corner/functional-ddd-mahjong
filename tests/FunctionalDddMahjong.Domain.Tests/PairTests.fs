namespace FunctionalDddMahjong.Domain.Tests

open Xunit
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.Domain.Pair

module PairTests =

    // テスト用のヘルパー関数
    let createTile tileType = create tileType

    [<Fact>]
    let ``tryCreatePair should create valid Character pair`` () =
        let tiles =
            [ createTile (Character Nine)
              createTile (Character Nine) ]

        let result = tryCreatePair tiles
        Assert.True(Result.isOk result)

        match result with
        | Ok pair ->
            Assert.Equal("Pair", getPairType pair)
            let pairTiles = getPairTiles pair
            Assert.Equal(2, List.length pairTiles)
        | Error _ -> Assert.True(false, "Expected Ok result")

    [<Fact>]
    let ``tryCreatePair should create valid Honor pair`` () =
        let tiles =
            [ createTile (Honor White)
              createTile (Honor White) ]

        let result = tryCreatePair tiles
        Assert.True(Result.isOk result)

    [<Fact>]
    let ``tryCreatePair should fail for different tiles`` () =
        let tiles =
            [ createTile (Character One)
              createTile (Character Two) ]

        let result = tryCreatePair tiles
        Assert.True(Result.isError result)

    [<Fact>]
    let ``tryCreatePair should fail for wrong tile count`` () =
        let tiles =
            [ createTile (Character Five)
              createTile (Character Five)
              createTile (Character Five) ]

        let result = tryCreatePair tiles
        Assert.True(Result.isError result)

    [<Fact>]
    let ``findAllPairs should detect all pairs in tile list`` () =
        let tiles =
            [ createTile (Character Nine)
              createTile (Character Nine)
              createTile (Honor White)
              createTile (Honor White)
              createTile (Circle One) ]

        let pairs = findAllPairs tiles
        Assert.Equal(2, List.length pairs)

    [<Fact>]
    let ``findAllPairs should return empty for empty tile list`` () =
        let emptyTiles = []
        let pairs = findAllPairs emptyTiles
        Assert.Empty(pairs)

    [<Fact>]
    let ``pairToString should display pair correctly`` () =
        let tiles =
            [ createTile (Honor White)
              createTile (Honor White) ]

        match tryCreatePair tiles with
        | Ok pair ->
            let str = pairToString pair
            Assert.Contains("Pair", str)
            Assert.Contains("白", str)
        | Error _ -> Assert.True(false, "Expected valid pair")
