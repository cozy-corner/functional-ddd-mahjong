module TileTests

open Xunit
open FunctionalDddMahjong.Domain.Tile

[<Fact>]
let ``create should return correct tile for Character`` () =
    let tile = create (Character One)

    match getValue tile with
    | Character One -> ()
    | _ -> failwith "Expected Character One"

[<Fact>]
let ``create should return correct tile for Circle`` () =
    let tile = create (Circle Five)

    match getValue tile with
    | Circle Five -> ()
    | _ -> failwith "Expected Circle Five"

[<Fact>]
let ``create should return correct tile for Bamboo`` () =
    let tile = create (Bamboo Nine)

    match getValue tile with
    | Bamboo Nine -> ()
    | _ -> failwith "Expected Bamboo Nine"

[<Fact>]
let ``create should return correct tile for Honor`` () =
    let tile = create (Honor East)

    match getValue tile with
    | Honor East -> ()
    | _ -> failwith "Expected Honor East"

[<Theory>]
[<InlineData("One")>]
[<InlineData("Five")>]
[<InlineData("Nine")>]
let ``getValue returns correct TileType for Character tiles`` (numberStr: string) =
    let numberValue =
        match numberStr with
        | "One" -> One
        | "Five" -> Five
        | "Nine" -> Nine
        | _ -> failwith "Invalid number"

    let tile = create (Character numberValue)

    match getValue tile with
    | Character n when n = numberValue -> ()
    | _ -> failwithf "Expected Character %s" numberStr

[<Theory>]
[<InlineData("One")>]
[<InlineData("Five")>]
[<InlineData("Nine")>]
let ``getValue returns correct TileType for Circle tiles`` (numberStr: string) =
    let numberValue =
        match numberStr with
        | "One" -> One
        | "Five" -> Five
        | "Nine" -> Nine
        | _ -> failwith "Invalid number"

    let tile = create (Circle numberValue)

    match getValue tile with
    | Circle n when n = numberValue -> ()
    | _ -> failwithf "Expected Circle %s" numberStr

[<Theory>]
[<InlineData("One")>]
[<InlineData("Five")>]
[<InlineData("Nine")>]
let ``getValue returns correct TileType for Bamboo tiles`` (numberStr: string) =
    let numberValue =
        match numberStr with
        | "One" -> One
        | "Five" -> Five
        | "Nine" -> Nine
        | _ -> failwith "Invalid number"

    let tile = create (Bamboo numberValue)

    match getValue tile with
    | Bamboo n when n = numberValue -> ()
    | _ -> failwithf "Expected Bamboo %s" numberStr

[<Theory>]
[<InlineData("East")>]
[<InlineData("South")>]
[<InlineData("West")>]
[<InlineData("North")>]
[<InlineData("White")>]
[<InlineData("Green")>]
[<InlineData("Red")>]
let ``getValue returns correct TileType for Honor tiles`` (honorStr: string) =
    let honorValue =
        match honorStr with
        | "East" -> East
        | "South" -> South
        | "West" -> West
        | "North" -> North
        | "White" -> White
        | "Green" -> Green
        | "Red" -> Red
        | _ -> failwith "Invalid honor"

    let tile = create (Honor honorValue)

    match getValue tile with
    | Honor h when h = honorValue -> ()
    | _ -> failwithf "Expected Honor %s" honorStr

[<Fact>]
let ``compare orders tiles by type Character < Circle < Bamboo < Honor`` () =
    let character = create (Character One)
    let circle = create (Circle One)
    let bamboo = create (Bamboo One)
    let honor = create (Honor East)

    Assert.True(compare character circle < 0)
    Assert.True(compare circle bamboo < 0)
    Assert.True(compare bamboo honor < 0)

[<Fact>]
let ``compare orders tiles by number within same type`` () =
    let tile1 = create (Character One)
    let tile5 = create (Character Five)
    let tile9 = create (Character Nine)

    Assert.True(compare tile1 tile5 < 0)
    Assert.True(compare tile5 tile9 < 0)
    Assert.Equal(0, compare tile5 (create (Character Five)))

[<Fact>]
let ``compare orders honor tiles correctly`` () =
    let east = create (Honor East)
    let south = create (Honor South)
    let white = create (Honor White)
    let green = create (Honor Green)

    Assert.True(compare east south < 0)
    Assert.True(compare white green < 0)
    Assert.True(compare south white < 0)

[<Theory>]
[<InlineData(1, "One")>]
[<InlineData(5, "Five")>]
[<InlineData(9, "Nine")>]
let ``tryCreateFromNumber creates Character tile for valid number`` (number: int, expectedStr: string) =
    let expected =
        match expectedStr with
        | "One" -> One
        | "Five" -> Five
        | "Nine" -> Nine
        | _ -> failwith "Invalid expected"

    match tryCreateFromNumber Character number with
    | Ok tile ->
        match getValue tile with
        | Character n when n = expected -> ()
        | _ -> failwithf "Expected Character %s" expectedStr
    | Error _ -> failwithf "Should create Character tile for number %d" number

[<Theory>]
[<InlineData(0)>]
[<InlineData(-1)>]
[<InlineData(10)>]
[<InlineData(99)>]
let ``tryCreateFromNumber returns error for invalid number`` (number: int) =
    match tryCreateFromNumber Character number with
    | Error(InvalidNumberValue n) when n = number -> ()
    | _ -> failwithf "Expected Error (InvalidNumberValue %d)" number

[<Theory>]
[<InlineData("1M", "Character", "One")>]
[<InlineData("5M", "Character", "Five")>]
[<InlineData("9M", "Character", "Nine")>]
[<InlineData("1m", "Character", "One")>]
let ``tryParseFromString parses Character tiles`` (input: string, expectedType: string, expectedNumber: string) =
    let expectedTileType =
        match expectedType, expectedNumber with
        | "Character", "One" -> Character One
        | "Character", "Five" -> Character Five
        | "Character", "Nine" -> Character Nine
        | _ -> failwith "Invalid expected"

    match tryParseFromString input with
    | Ok tile ->
        match getValue tile with
        | tileType when tileType = expectedTileType -> ()
        | _ -> failwithf "Expected %s %s for input %s" expectedType expectedNumber input
    | Error _ -> failwithf "Should parse %s successfully" input

[<Theory>]
[<InlineData("1P", "Circle", "One")>]
[<InlineData("5P", "Circle", "Five")>]
[<InlineData("9P", "Circle", "Nine")>]
[<InlineData("1p", "Circle", "One")>]
let ``tryParseFromString parses Circle tiles`` (input: string, expectedType: string, expectedNumber: string) =
    let expectedTileType =
        match expectedType, expectedNumber with
        | "Circle", "One" -> Circle One
        | "Circle", "Five" -> Circle Five
        | "Circle", "Nine" -> Circle Nine
        | _ -> failwith "Invalid expected"

    match tryParseFromString input with
    | Ok tile ->
        match getValue tile with
        | tileType when tileType = expectedTileType -> ()
        | _ -> failwithf "Expected %s %s for input %s" expectedType expectedNumber input
    | Error _ -> failwithf "Should parse %s successfully" input

[<Theory>]
[<InlineData("1S", "Bamboo", "One")>]
[<InlineData("5S", "Bamboo", "Five")>]
[<InlineData("9S", "Bamboo", "Nine")>]
[<InlineData("1s", "Bamboo", "One")>]
let ``tryParseFromString parses Bamboo tiles`` (input: string, expectedType: string, expectedNumber: string) =
    let expectedTileType =
        match expectedType, expectedNumber with
        | "Bamboo", "One" -> Bamboo One
        | "Bamboo", "Five" -> Bamboo Five
        | "Bamboo", "Nine" -> Bamboo Nine
        | _ -> failwith "Invalid expected"

    match tryParseFromString input with
    | Ok tile ->
        match getValue tile with
        | tileType when tileType = expectedTileType -> ()
        | _ -> failwithf "Expected %s %s for input %s" expectedType expectedNumber input
    | Error _ -> failwithf "Should parse %s successfully" input

[<Theory>]
[<InlineData("E", "East")>]
[<InlineData("S", "South")>]
[<InlineData("W", "West")>]
[<InlineData("N", "North")>]
[<InlineData("e", "East")>]
let ``tryParseFromString parses Wind tiles`` (input: string, expectedHonor: string) =
    let expected =
        match expectedHonor with
        | "East" -> East
        | "South" -> South
        | "West" -> West
        | "North" -> North
        | _ -> failwith "Invalid honor"

    match tryParseFromString input with
    | Ok tile ->
        match getValue tile with
        | Honor h when h = expected -> ()
        | _ -> failwithf "Expected Honor %s for input %s" expectedHonor input
    | Error _ -> failwithf "Should parse %s successfully" input

[<Theory>]
[<InlineData("WH", "White")>]
[<InlineData("GR", "Green")>]
[<InlineData("RD", "Red")>]
[<InlineData("wh", "White")>]
let ``tryParseFromString parses Dragon tiles`` (input: string, expectedHonor: string) =
    let expected =
        match expectedHonor with
        | "White" -> White
        | "Green" -> Green
        | "Red" -> Red
        | _ -> failwith "Invalid honor"

    match tryParseFromString input with
    | Ok tile ->
        match getValue tile with
        | Honor h when h = expected -> ()
        | _ -> failwithf "Expected Honor %s for input %s" expectedHonor input
    | Error _ -> failwithf "Should parse %s successfully" input

[<Theory>]
[<InlineData("")>]
[<InlineData("1")>]
[<InlineData("M")>]
[<InlineData("10M")>]
[<InlineData("1X")>]
[<InlineData("ABC")>]
let ``tryParseFromString returns error for invalid format`` (input: string) =
    match tryParseFromString input with
    | Error(InvalidTileString _) -> ()
    | _ -> failwithf "Expected Error (InvalidTileString) for input: %s" input

[<Theory>]
[<InlineData("One", "1萬")>]
[<InlineData("Five", "5萬")>]
[<InlineData("Nine", "9萬")>]
let ``toString returns correct string for Character tiles`` (numberStr: string, expected: string) =
    let numberValue =
        match numberStr with
        | "One" -> One
        | "Five" -> Five
        | "Nine" -> Nine
        | _ -> failwith "Invalid number"

    let tile = create (Character numberValue)
    Assert.Equal(expected, toString tile)

[<Theory>]
[<InlineData("One", "1筒")>]
[<InlineData("Five", "5筒")>]
[<InlineData("Nine", "9筒")>]
let ``toString returns correct string for Circle tiles`` (numberStr: string, expected: string) =
    let numberValue =
        match numberStr with
        | "One" -> One
        | "Five" -> Five
        | "Nine" -> Nine
        | _ -> failwith "Invalid number"

    let tile = create (Circle numberValue)
    Assert.Equal(expected, toString tile)

[<Theory>]
[<InlineData("One", "1索")>]
[<InlineData("Five", "5索")>]
[<InlineData("Nine", "9索")>]
let ``toString returns correct string for Bamboo tiles`` (numberStr: string, expected: string) =
    let numberValue =
        match numberStr with
        | "One" -> One
        | "Five" -> Five
        | "Nine" -> Nine
        | _ -> failwith "Invalid number"

    let tile = create (Bamboo numberValue)
    Assert.Equal(expected, toString tile)

[<Theory>]
[<InlineData("East", "東")>]
[<InlineData("South", "南")>]
[<InlineData("West", "西")>]
[<InlineData("North", "北")>]
[<InlineData("White", "白")>]
[<InlineData("Green", "發")>]
[<InlineData("Red", "中")>]
let ``toString returns correct string for Honor tiles`` (honorStr: string, expected: string) =
    let honorValue =
        match honorStr with
        | "East" -> East
        | "South" -> South
        | "West" -> West
        | "North" -> North
        | "White" -> White
        | "Green" -> Green
        | "Red" -> Red
        | _ -> failwith "Invalid honor"

    let tile = create (Honor honorValue)
    Assert.Equal(expected, toString tile)
