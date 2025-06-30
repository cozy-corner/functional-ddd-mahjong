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
    | Error _ -> failwith "Should succeed with 13 tiles"

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
    | Error(InvalidTileCount(12, _)) -> ()
    | Ok _ -> failwith "Should fail with 12 tiles"
    | Error _ -> failwith "Should return InvalidTileCount error"

[<Fact>]
let ``draw on Waiting hand adds tile and returns Ready hand`` () =
    match createWaitingHand () with
    | Ok(Waiting _ as waitingHand) ->
        let newTile = createTile (Honor White)
        let readyHand = draw newTile waitingHand

        Assert.Equal(14, count readyHand)
        Assert.True(isReady readyHand)
        Assert.True(contains newTile readyHand)
    | Ok(Ready _) -> failwith "Expected Waiting hand"
    | Error _ -> failwith "Failed to create initial hand"

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
        | Error _ -> failwith "Should succeed discarding from Ready hand"
    | Ok(Ready _) -> failwith "Expected Waiting hand"
    | Error _ -> failwith "Failed to create initial hand"

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
    | Error _ -> failwith "Failed to create initial hand"

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
    | Error _ -> failwith "Failed to create hand"

[<Fact>]
let ``discard non-existent tile returns TileNotFound error`` () =
    match createWaitingHand () with
    | Ok(Waiting _ as waitingHand) ->
        let drawnTile = createTile (Honor White)
        let readyHand = draw drawnTile waitingHand

        match discard (createTile (Honor Green)) readyHand with
        | Error(TileNotFound _) -> ()
        | Ok _ -> failwith "Should fail discarding non-existent tile"
        | Error _ -> failwith "Should return TileNotFound error"
    | Ok(Ready _) -> failwith "Expected Waiting hand"
    | Error _ -> failwith "Failed to create initial hand"
