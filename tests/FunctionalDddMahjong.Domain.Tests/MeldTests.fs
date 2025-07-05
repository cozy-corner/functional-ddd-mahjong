namespace FunctionalDddMahjong.Domain.Tests

open Xunit
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.Domain.Meld

module MeldTests =

    // テスト用のヘルパー関数
    let createTile tileType = create tileType

    // 文字列から数値への変換ヘルパー
    let private parseNumberValue (numberStr: string) =
        match numberStr with
        | "One" -> One
        | "Two" -> Two
        | "Three" -> Three
        | "Four" -> Four
        | "Five" -> Five
        | "Six" -> Six
        | "Seven" -> Seven
        | "Eight" -> Eight
        | "Nine" -> Nine
        | _ -> failwith $"Invalid number: {numberStr}"

    // 文字列から名誉牌への変換ヘルパー
    let private parseHonorValue (honorStr: string) =
        match honorStr with
        | "East" -> East
        | "South" -> South
        | "West" -> West
        | "North" -> North
        | "White" -> White
        | "Green" -> Green
        | "Red" -> Red
        | _ -> failwith $"Invalid honor: {honorStr}"

    // 数字牌作成ヘルパー
    let private createNumberTile (tileType: string) (numberValue: NumberValue) =
        match tileType with
        | "Character" -> createTile (Character numberValue)
        | "Circle" -> createTile (Circle numberValue)
        | "Bamboo" -> createTile (Bamboo numberValue)
        | _ -> failwith $"Invalid tile type: {tileType}"

    // 順子テスト
    module SequenceTests =

        [<Fact>]
        let ``tryCreateSequence should create valid Character sequence`` () =
            let tiles =
                [ createTile (Character One)
                  createTile (Character Two)
                  createTile (Character Three) ]

            let result =
                FunctionalDddMahjong.Domain.Meld.tryCreateSequence tiles

            Assert.True(Result.isOk result)

            match result with
            | Ok meld ->
                Assert.Equal("Sequence", getMeldType meld)
                let meldTiles = getMeldTiles meld
                Assert.Equal(3, List.length meldTiles)
            | Error _ -> Assert.True(false, "Expected Ok result")

        [<Fact>]
        let ``tryCreateSequence should create valid Circle sequence`` () =
            let tiles =
                [ createTile (Circle Seven)
                  createTile (Circle Eight)
                  createTile (Circle Nine) ]

            let result =
                FunctionalDddMahjong.Domain.Meld.tryCreateSequence tiles

            Assert.True(Result.isOk result)

        [<Fact>]
        let ``tryCreateSequence should create valid Bamboo sequence`` () =
            let tiles =
                [ createTile (Bamboo Four)
                  createTile (Bamboo Five)
                  createTile (Bamboo Six) ]

            let result =
                FunctionalDddMahjong.Domain.Meld.tryCreateSequence tiles

            Assert.True(Result.isOk result)

        [<Theory>]
        [<InlineData(1, 3, 5)>] // Non-consecutive
        [<InlineData(1, 1, 1)>] // Same tiles
        [<InlineData(7, 8, 1)>] // Wrong order (but will be sorted internally)
        let ``tryCreateSequence should fail for invalid Character sequences`` (n1, n2, n3) =
            let numberValues =
                [ One
                  Two
                  Three
                  Four
                  Five
                  Six
                  Seven
                  Eight
                  Nine ]

            let tiles =
                [ createTile (Character(List.item (n1 - 1) numberValues))
                  createTile (Character(List.item (n2 - 1) numberValues))
                  createTile (Character(List.item (n3 - 1) numberValues)) ]

            let result =
                FunctionalDddMahjong.Domain.Meld.tryCreateSequence tiles

            Assert.True(Result.isError result)

        [<Fact>]
        let ``tryCreateSequence should fail for mixed tile types`` () =
            let tiles =
                [ createTile (Character One)
                  createTile (Circle Two)
                  createTile (Bamboo Three) ]

            let result =
                FunctionalDddMahjong.Domain.Meld.tryCreateSequence tiles

            Assert.True(Result.isError result)

        [<Fact>]
        let ``tryCreateSequence should fail for Honor tiles`` () =
            let tiles =
                [ createTile (Honor East)
                  createTile (Honor South)
                  createTile (Honor West) ]

            let result =
                FunctionalDddMahjong.Domain.Meld.tryCreateSequence tiles

            Assert.True(Result.isError result)

        [<Fact>]
        let ``tryCreateSequence should fail for insufficient tiles`` () =
            let tiles =
                [ createTile (Character One)
                  createTile (Character Two) ]

            let result =
                FunctionalDddMahjong.Domain.Meld.tryCreateSequence tiles

            Assert.True(Result.isError result)

    // 刻子テスト
    module TripletTests =

        [<Fact>]
        let ``tryCreateTriplet should create valid Character triplet`` () =
            let tiles =
                [ createTile (Character Five)
                  createTile (Character Five)
                  createTile (Character Five) ]

            let result = tryCreateTriplet tiles
            Assert.True(Result.isOk result)

            match result with
            | Ok meld ->
                Assert.Equal("Triplet", getMeldType meld)
                let meldTiles = getMeldTiles meld
                Assert.Equal(3, List.length meldTiles)
            | Error _ -> Assert.True(false, "Expected Ok result")

        [<Fact>]
        let ``tryCreateTriplet should create valid Honor triplet`` () =
            let tiles =
                [ createTile (Honor East)
                  createTile (Honor East)
                  createTile (Honor East) ]

            let result = tryCreateTriplet tiles
            Assert.True(Result.isOk result)

        [<Fact>]
        let ``tryCreateTriplet should fail for different tiles`` () =
            let tiles =
                [ createTile (Character One)
                  createTile (Character Two)
                  createTile (Character Three) ]

            let result = tryCreateTriplet tiles
            Assert.True(Result.isError result)

        [<Fact>]
        let ``tryCreateTriplet should fail for insufficient tiles`` () =
            let tiles =
                [ createTile (Character Five)
                  createTile (Character Five) ]

            let result = tryCreateTriplet tiles
            Assert.True(Result.isError result)


    // 複合検出テスト
    module DetectionTests =

        [<Fact>]
        let ``findAllMelds should detect all melds in tile list`` () =
            let tiles =
                [ createTile (Character One)
                  createTile (Character Two)
                  createTile (Character Three)
                  createTile (Circle Four)
                  createTile (Circle Five)
                  createTile (Circle Six)
                  createTile (Honor East)
                  createTile (Honor East)
                  createTile (Honor East) ]

            let melds = findAllMelds tiles
            Assert.Equal(3, List.length melds) // 2 sequences + 1 triplet


        [<Fact>]
        let ``detection functions should return empty for empty tile list`` () =
            let emptyTiles = []

            let melds = findAllMelds emptyTiles

            Assert.Empty(melds)

    // toString テスト
    module DisplayTests =

        [<Fact>]
        let ``meldToString should display sequence correctly`` () =
            let tiles =
                [ createTile (Character One)
                  createTile (Character Two)
                  createTile (Character Three) ]

            match tryCreateSequence tiles with
            | Ok meld ->
                let str = meldToString meld
                Assert.Contains("Sequence", str)
                Assert.Contains("1萬", str)
                Assert.Contains("2萬", str)
                Assert.Contains("3萬", str)
            | Error _ -> Assert.True(false, "Expected valid sequence")

        [<Fact>]
        let ``meldToString should display triplet correctly`` () =
            let tiles =
                [ createTile (Honor East)
                  createTile (Honor East)
                  createTile (Honor East) ]

            match tryCreateTriplet tiles with
            | Ok meld ->
                let str = meldToString meld
                Assert.Contains("Triplet", str)
                Assert.Contains("東", str)
            | Error _ -> Assert.True(false, "Expected valid triplet")

        [<Theory>]
        [<InlineData("Character,One,Two,Three", "123m")>]
        [<InlineData("Circle,Five,Six,Seven", "567p")>]
        [<InlineData("Bamboo,Seven,Eight,Nine", "789s")>]
        let ``meldToShortString should display sequence correctly`` (tilesData: string, expected: string) =
            let parts = tilesData.Split(',')
            let tileType = parts.[0]
            let numbers = parts.[1..3]

            let tiles =
                numbers
                |> Array.map (fun numStr ->
                    let numberValue = parseNumberValue numStr
                    createNumberTile tileType numberValue)
                |> Array.toList

            match tryCreateSequence tiles with
            | Ok meld ->
                let str = meldToShortString meld
                Assert.Equal(expected, str)
            | Error _ -> Assert.True(false, "Expected valid sequence")

        [<Theory>]
        [<InlineData("Character,Five", "555m")>]
        [<InlineData("Circle,Two", "222p")>]
        [<InlineData("Bamboo,Nine", "999s")>]
        let ``meldToShortString should display number triplet correctly`` (tileData: string, expected: string) =
            let parts = tileData.Split(',')
            let tileType = parts.[0]
            let numberStr = parts.[1]

            let numberValue = parseNumberValue numberStr
            let tile = createNumberTile tileType numberValue

            let tiles = [ tile; tile; tile ]

            match tryCreateTriplet tiles with
            | Ok meld ->
                let str = meldToShortString meld
                Assert.Equal(expected, str)
            | Error _ -> Assert.True(false, "Expected valid triplet")

        [<Theory>]
        [<InlineData("East", "EEE")>]
        [<InlineData("South", "SSS")>]
        [<InlineData("West", "WWW")>]
        [<InlineData("North", "NNN")>]
        [<InlineData("White", "WHWHWH")>]
        [<InlineData("Green", "GRGRGR")>]
        [<InlineData("Red", "RDRDRD")>]
        let ``meldToShortString should display honor triplet correctly`` (honorStr: string, expected: string) =
            let honorValue = parseHonorValue honorStr
            let tile = createTile (Honor honorValue)
            let tiles = [ tile; tile; tile ]

            match tryCreateTriplet tiles with
            | Ok meld ->
                let str = meldToShortString meld
                Assert.Equal(expected, str)
            | Error _ -> Assert.True(false, "Expected valid triplet")
