namespace FunctionalDddMahjong.Domain.Tests

open System
open Xunit
open FunctionalDddMahjong.Domain

module ReachTests =

    module TestHelpers =
        let createContext score turn reachStatus =
            match Score.create score, Turn.create turn with
            | Ok scoreValue, Ok turnValue ->
                { PlayerScore = scoreValue
                  CurrentTurn = turnValue
                  ReachStatus = reachStatus }
            | Error scoreErr, Ok _ -> failwith $"Invalid score: {scoreErr}"
            | Ok _, Error turnErr -> failwith $"Invalid turn: {turnErr}"
            | Error scoreErr, Error turnErr -> failwith $"Invalid score and turn: {scoreErr}, {turnErr}"

        let private parseTiles (tilesStr: string) =
            tilesStr.Split(',')
            |> Array.toList
            |> List.map (fun s ->
                match Tile.tryParseFromString s with
                | Ok tile -> tile
                | Error err -> failwith (sprintf "Failed to parse tile '%s': %A" s err))

        let createTenpaiHand () =
            let tiles =
                parseTiles "E,E,E,S,S,S,W,W,W,H,H,G,G"

            match Hand.tryCreateFromDeal tiles with
            | Ok hand -> hand
            | Error _ -> failwith "Invalid tenpai hand"

        let createNonTenpaiHand () =
            let tiles =
                parseTiles "E,E,E,S,S,W,W,N,N,H,H,G,R"

            match Hand.tryCreateFromDeal tiles with
            | Ok hand -> hand
            | Error _ -> failwith "Invalid non-tenpai hand"

    [<Fact>]
    let ``declareReach should succeed with valid conditions`` () =
        let hand = TestHelpers.createTenpaiHand ()

        let context =
            TestHelpers.createContext 25000 5 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReach hand context

        match result with
        | Ok(reachType, newScore) ->
            Assert.Equal(Reach, reachType)
            Assert.Equal(24000, Score.value newScore)
        | Error err -> Assert.Fail($"Should succeed but got error: {err}")

    [<Fact>]
    let ``declareReach should return DoubleReach on first turn`` () =
        let hand = TestHelpers.createTenpaiHand ()

        let context =
            TestHelpers.createContext 25000 1 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReach hand context

        match result with
        | Ok(reachType, newScore) ->
            Assert.Equal(DoubleReach, reachType)
            Assert.Equal(24000, Score.value newScore)
        | Error err -> Assert.Fail($"Should succeed but got error: {err}")

    [<Fact>]
    let ``declareReach should fail when not tenpai`` () =
        let hand =
            TestHelpers.createNonTenpaiHand ()

        let context =
            TestHelpers.createContext 25000 5 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReach hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error error -> Assert.Equal(NotTenpai, error)

    [<Fact>]
    let ``declareReach should fail with insufficient score`` () =
        let hand = TestHelpers.createTenpaiHand ()

        let context =
            TestHelpers.createContext 500 5 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReach hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error error -> Assert.Equal(InsufficientScore, error)

    [<Fact>]
    let ``declareReach should fail when too late in game`` () =
        let hand = TestHelpers.createTenpaiHand ()

        let context =
            TestHelpers.createContext 25000 16 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReach hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error error -> Assert.Equal(TooLateInGame, error)

    [<Fact>]
    let ``declareReach should fail when already reached`` () =
        let hand = TestHelpers.createTenpaiHand ()

        let context =
            TestHelpers.createContext 25000 5 ReachStatus.AlreadyReached

        let result =
            ReachDeclaration.declareReach hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error error -> Assert.Equal(AlreadyReached, error)

    [<Theory>]
    [<InlineData(1000, true)>]
    [<InlineData(999, false)>]
    [<InlineData(0, false)>]
    [<InlineData(25000, true)>]
    let ``checkScore should validate minimum score requirement`` (score: int) (shouldSucceed: bool) =
        let context =
            TestHelpers.createContext score 5 ReachStatus.NotReached

        let result =
            ReachValidation.checkScore context

        if shouldSucceed then
            Assert.True(Result.isOk result)
        else
            match result with
            | Error InsufficientScore -> Assert.True(true)
            | _ -> Assert.Fail("Should return InsufficientScore error")

    [<Theory>]
    [<InlineData(1, true)>]
    [<InlineData(15, true)>]
    [<InlineData(16, false)>]
    [<InlineData(18, false)>]
    let ``checkGameStage should validate turn limit`` (turn: int) (shouldSucceed: bool) =
        let context =
            TestHelpers.createContext 25000 turn ReachStatus.NotReached

        let result =
            ReachValidation.checkGameStage context

        if shouldSucceed then
            Assert.True(Result.isOk result)
        else
            match result with
            | Error TooLateInGame -> Assert.True(true)
            | _ -> Assert.Fail("Should return TooLateInGame error")

    // エラー集約版のテスト
    [<Fact>]
    let ``declareReachWithAllErrors should succeed with valid conditions`` () =
        let hand = TestHelpers.createTenpaiHand ()

        let context =
            TestHelpers.createContext 25000 5 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReachWithAllErrors hand context

        match result with
        | Ok(reachType, newScore) ->
            Assert.Equal(Reach, reachType)
            Assert.Equal(24000, Score.value newScore)
        | Error errors -> Assert.Fail($"Should succeed but got errors: {errors}")

    [<Fact>]
    let ``declareReachWithAllErrors should collect single error`` () =
        let hand =
            TestHelpers.createNonTenpaiHand ()

        let context =
            TestHelpers.createContext 25000 5 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReachWithAllErrors hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error errors ->
            Assert.Equal(1, List.length errors)
            Assert.Equal(NotTenpai, List.head errors)

    [<Fact>]
    let ``declareReachWithAllErrors should collect multiple errors`` () =
        let hand =
            TestHelpers.createNonTenpaiHand ()

        let context =
            TestHelpers.createContext 500 17 ReachStatus.AlreadyReached

        let result =
            ReachDeclaration.declareReachWithAllErrors hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error errors ->
            Assert.Equal(4, List.length errors)
            Assert.Contains(NotTenpai, errors)
            Assert.Contains(InsufficientScore, errors)
            Assert.Contains(TooLateInGame, errors)
            Assert.Contains(AlreadyReached, errors)

    [<Fact>]
    let ``declareReachWithAllErrors should collect two errors`` () =
        let hand = TestHelpers.createTenpaiHand ()

        let context =
            TestHelpers.createContext 500 17 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReachWithAllErrors hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error errors ->
            Assert.Equal(2, List.length errors)
            Assert.Contains(InsufficientScore, errors)
            Assert.Contains(TooLateInGame, errors)

    [<Fact>]
    let ``declareReachWithAllErrors should collect three errors`` () =
        let hand =
            TestHelpers.createNonTenpaiHand ()

        let context =
            TestHelpers.createContext 500 17 ReachStatus.NotReached

        let result =
            ReachDeclaration.declareReachWithAllErrors hand context

        match result with
        | Ok(reachType, score) -> Assert.Fail($"Should fail but got success: {reachType}, {Score.value score}")
        | Error errors ->
            Assert.Equal(3, List.length errors)
            Assert.Contains(NotTenpai, errors)
            Assert.Contains(InsufficientScore, errors)
            Assert.Contains(TooLateInGame, errors)
