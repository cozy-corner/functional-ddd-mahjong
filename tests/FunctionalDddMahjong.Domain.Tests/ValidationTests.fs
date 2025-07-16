namespace FunctionalDddMahjong.Domain.Tests

open Xunit
open FunctionalDddMahjong.Domain
open FunctionalDddMahjong.SharedKernel

module ValidationTests =

    [<Fact>]
    let ``sequence should return Ok with all success values`` () =
        let results = [ Ok 1; Ok 2; Ok 3 ]

        let result = Validation.sequence results

        match result with
        | Ok values -> Assert.Equal<int list>([ 1; 2; 3 ], values)
        | Error errors -> Assert.Fail($"Should succeed but got errors: {errors}")

    [<Fact>]
    let ``sequence should return all errors when all fail`` () =
        let results =
            [ Error "error1"
              Error "error2"
              Error "error3" ]

        let result = Validation.sequence results

        match result with
        | Ok values -> Assert.Fail($"Should fail but got values: {values}")
        | Error errors -> Assert.Equal<string list>([ "error1"; "error2"; "error3" ], errors)

    [<Fact>]
    let ``sequence should return all errors when some fail`` () =
        let results =
            [ Ok 1
              Error "error1"
              Ok 2
              Error "error2" ]

        let result = Validation.sequence results

        match result with
        | Ok values -> Assert.Fail($"Should fail but got values: {values}")
        | Error errors -> Assert.Equal<string list>([ "error1"; "error2" ], errors)

    [<Fact>]
    let ``traverse should apply function and sequence results`` () =
        let parseInt (s: string) =
            match System.Int32.TryParse(s) with
            | true, i -> Ok i
            | false, _ -> Error $"Invalid number: {s}"

        let strings = [ "1"; "2"; "3" ]

        let result =
            Validation.traverse parseInt strings

        match result with
        | Ok values -> Assert.Equal<int list>([ 1; 2; 3 ], values)
        | Error errors -> Assert.Fail($"Should succeed but got errors: {errors}")

    [<Fact>]
    let ``traverse should collect all errors`` () =
        let parseInt (s: string) =
            match System.Int32.TryParse(s) with
            | true, i -> Ok i
            | false, _ -> Error $"Invalid number: {s}"

        let strings = [ "1"; "abc"; "3"; "def" ]

        let result =
            Validation.traverse parseInt strings

        match result with
        | Ok values -> Assert.Fail($"Should fail but got values: {values}")
        | Error errors ->
            Assert.Equal(2, List.length errors)
            Assert.Contains("Invalid number: abc", errors)
            Assert.Contains("Invalid number: def", errors)

    [<Fact>]
    let ``apply should combine two success values`` () =
        let fResult = Ok(fun x y -> x + y)
        let xResult = Ok 10
        let yResult = Ok 5

        let result =
            fResult |> Validation.apply <| xResult
            |> Validation.apply
            <| (yResult |> Validation.liftError)

        match result with
        | Ok value -> Assert.Equal(15, value)
        | Error errors -> Assert.Fail($"Should succeed but got errors: {errors}")

    [<Fact>]
    let ``apply should collect all errors when both fail`` () =
        let fResult = Error [ "func error" ]
        let xResult = Error [ "x error" ]

        let result =
            Validation.apply fResult xResult

        match result with
        | Ok value -> Assert.Fail($"Should fail but got value: {value}")
        | Error errors -> Assert.Equal<string list>([ "func error"; "x error" ], errors)

    [<Fact>]
    let ``liftError should convert single error to error list`` () =
        let result = Error "single error"

        let lifted = Validation.liftError result

        match lifted with
        | Ok value -> Assert.Fail($"Should fail but got value: {value}")
        | Error errors -> Assert.Equal<string list>([ "single error" ], errors)

    [<Fact>]
    let ``validateAll should succeed when all validations pass`` () =
        let validations =
            [ fun () -> Ok()
              fun () -> Ok()
              fun () -> Ok() ]

        let result =
            Validation.validateAll validations

        match result with
        | Ok() -> Assert.True(true)
        | Error errors -> Assert.Fail($"Should succeed but got errors: {errors}")

    [<Fact>]
    let ``validateAll should collect all errors when validations fail`` () =
        let validations =
            [ fun () -> Ok()
              fun () -> Error "error1"
              fun () -> Error "error2"
              fun () -> Ok()
              fun () -> Error "error3" ]

        let result =
            Validation.validateAll validations

        match result with
        | Ok() -> Assert.Fail("Should fail but succeeded")
        | Error errors ->
            Assert.Equal(3, List.length errors)
            Assert.Contains("error1", errors)
            Assert.Contains("error2", errors)
            Assert.Contains("error3", errors)
