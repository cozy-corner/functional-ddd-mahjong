module HandTests

open Xunit
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.Domain.Hand

// Test helper functions
let createTile tileType = create tileType

let createWaitingHand () =
    let tiles =
        [ createTile (Character One)
          createTile (Character Two)
          createTile (Character Three)
          createTile (Circle One)
          createTile (Circle Two)
          createTile (Circle Three)
          createTile (Bamboo One)
          createTile (Bamboo Two)
          createTile (Bamboo Three)
          createTile (Honor East)
          createTile (Honor South)
          createTile (Honor West)
          createTile (Honor North) ]

    tryCreateFromDeal tiles

// Hand creation tests
[<Fact>]
let ``tryCreateFromDeal with 13 tiles returns Ok Waiting hand`` () =
    match createWaitingHand () with
    | Ok hand ->
        Assert.Equal(13, count hand)
        Assert.True(isWaiting hand)
    | Error e -> failwith $"Should succeed with 13 tiles, but got error: {e}"

[<Fact>]
let ``tryCreateFromDeal with 12 tiles returns InvalidTileCount error`` () =
    let tiles =
        [ createTile (Character One)
          createTile (Character Two)
          createTile (Character Three)
          createTile (Circle One)
          createTile (Circle Two)
          createTile (Circle Three)
          createTile (Bamboo One)
          createTile (Bamboo Two)
          createTile (Bamboo Three)
          createTile (Honor East)
          createTile (Honor South)
          createTile (Honor West) ]

    match tryCreateFromDeal tiles with
    | Error(InvalidTileCount(actual, expected)) ->
        Assert.Equal(12, actual)
        Assert.Equal("13 (initial deal)", expected)
    | Ok _ -> failwith "Should fail with 12 tiles"
    | Error e -> failwith $"Should return InvalidTileCount error, but got {e}"

[<Fact>]
let ``draw on Waiting hand adds tile and returns Ready hand`` () =
    match createWaitingHand () with
    | Ok(Waiting _ as waitingHand) ->
        let newTile = createTile (Honor White)
        let readyHand = draw newTile waitingHand

        Assert.Equal(14, count readyHand)
        Assert.True(isReady readyHand)
        Assert.True(contains newTile readyHand)
    | Ok(Ready _) -> failwith "Expected Waiting hand, but got Ready hand"
    | Error e -> failwith $"Failed to create initial hand: {e}"

[<Fact>]
let ``discard on Ready hand removes tile and returns Waiting hand`` () =
    match createWaitingHand () with
    | Ok(Waiting _ as waitingHand) ->
        let tileToDiscard = createTile (Honor White)

        let readyHand =
            draw tileToDiscard waitingHand

        match discard tileToDiscard readyHand with
        | Ok newHand ->
            Assert.Equal(13, count newHand)
            Assert.True(isWaiting newHand)
            Assert.False(contains tileToDiscard newHand)
        | Error e -> failwith $"Should succeed discarding from Ready hand, but got: {e}"
    | Ok(Ready _) -> failwith "Expected Waiting hand, but got Ready hand"
    | Error e -> failwith $"Failed to create initial hand: {e}"

[<Fact>]
let ``contains returns true for existing tile and false for non-existing tile`` () =
    match createWaitingHand () with
    | Ok hand ->
        let existingTile =
            createTile (Character One)

        let nonExistentTile =
            createTile (Honor White)

        Assert.True(contains existingTile hand)
        Assert.False(contains nonExistentTile hand)
    | Error e -> failwith $"Failed to create initial hand: {e}"

[<Fact>]
let ``sort arranges tiles in order Characters then Circles then Bamboos then Honors`` () =
    let tiles =
        [ createTile (Honor East)
          createTile (Character Three)
          createTile (Circle One)
          createTile (Bamboo Two)
          createTile (Character One)
          createTile (Circle Three)
          createTile (Bamboo One)
          createTile (Character Two)
          createTile (Circle Two)
          createTile (Bamboo Three)
          createTile (Honor South)
          createTile (Honor West)
          createTile (Honor North) ]

    match tryCreateFromDeal tiles with
    | Ok hand ->
        let sortedHand = sort hand
        let sortedTiles = getTiles sortedHand
        Assert.Equal(createTile (Character One), sortedTiles.[0])
        Assert.Equal(createTile (Character Two), sortedTiles.[1])
        Assert.Equal(createTile (Character Three), sortedTiles.[2])
        Assert.Equal(createTile (Circle One), sortedTiles.[3])
    | Error e -> failwith $"Failed to create hand: {e}"

[<Fact>]
let ``discard non-existent tile returns TileNotFound error`` () =
    match createWaitingHand () with
    | Ok(Waiting _ as waitingHand) ->
        let drawnTile = createTile (Honor White)
        let readyHand = draw drawnTile waitingHand

        let nonExistentTile =
            createTile (Honor Green)

        match discard nonExistentTile readyHand with
        | Error(TileNotFound tile) -> Assert.Equal(nonExistentTile, tile)
        | Ok _ -> failwith "Should fail discarding non-existent tile"
        | Error e -> failwith $"Should return TileNotFound error, but got {e}"
    | Ok(Ready _) -> failwith "Expected Waiting hand, but got Ready hand"
    | Error e -> failwith $"Failed to create initial hand: {e}"

// countTile function tests
[<Fact>]
let ``countTile returns 3 when hand contains three identical tiles`` () =
    let tiles =
        [ createTile (Character One)
          createTile (Character One)
          createTile (Character One)
          createTile (Character Two)
          createTile (Circle One)
          createTile (Circle Two)
          createTile (Circle Three)
          createTile (Bamboo One)
          createTile (Bamboo Two)
          createTile (Bamboo Three)
          createTile (Honor East)
          createTile (Honor South)
          createTile (Honor West) ]

    match tryCreateFromDeal tiles with
    | Ok hand -> Assert.Equal(3, countTile (createTile (Character One)) hand)
    | Error e -> failwith $"Failed to create hand: {e}"

[<Fact>]
let ``countTile returns 1 when hand contains single tile`` () =
    match createWaitingHand () with
    | Ok hand ->
        // createWaitingHand creates a hand with one of each tile
        Assert.Equal(1, countTile (createTile (Character One)) hand)
        Assert.Equal(1, countTile (createTile (Honor East)) hand)
    | Error e -> failwith $"Failed to create hand: {e}"

[<Fact>]
let ``countTile returns 0 when tile not in hand`` () =
    match createWaitingHand () with
    | Ok hand ->
        let nonExistentTile =
            createTile (Honor White)

        Assert.Equal(0, countTile nonExistentTile hand)
    | Error e -> failwith $"Failed to create hand: {e}"

[<Fact>]
let ``countTile increments count after drawing same tile`` () =
    match createWaitingHand () with
    | Ok(Waiting _ as waitingHand) ->
        let existingTile =
            createTile (Character One)

        let countBefore =
            countTile existingTile waitingHand

        let readyHand =
            draw existingTile waitingHand

        let countAfter =
            countTile existingTile readyHand

        Assert.Equal(countBefore + 1, countAfter)
    | Ok(Ready _) -> failwith "Expected Waiting hand, but got Ready hand"
    | Error e -> failwith $"Failed to create initial hand: {e}"

// Edge case tests
[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(12)>]
[<InlineData(14)>]
[<InlineData(15)>]
[<InlineData(20)>]
let ``tryCreateFromDeal returns InvalidTileCount error when tile count is not 13`` (tileCount: int) =
    let tiles =
        List.init tileCount (fun _ -> createTile (Character One))

    match tryCreateFromDeal tiles with
    | Error(InvalidTileCount(actual, expected)) ->
        Assert.Equal(tileCount, actual)
        Assert.Contains("13", expected)
    | Ok _ -> failwith $"Should fail with {tileCount} tiles"

[<Fact>]
let ``isWaiting returns true for 13-tile hand and isReady returns true for 14-tile hand`` () =
    match createWaitingHand () with
    | Ok(Waiting _ as waitingHand) ->
        // 13-tile Waiting hand
        Assert.True(isWaiting waitingHand)
        Assert.False(isReady waitingHand)

        // 14-tile Ready hand after draw
        let readyHand =
            draw (createTile (Honor White)) waitingHand

        Assert.False(isWaiting readyHand)
        Assert.True(isReady readyHand)
    | Ok(Ready _) -> failwith "Expected Waiting hand, but got Ready hand"
    | Error e -> failwith $"Failed to create hand: {e}"
