namespace FunctionalDddMahjong.Api

open FunctionalDddMahjong.Domain
open FunctionalDddMahjong.ApplicationServices

/// Handles the conversion and validation of reach declaration requests
module ReachDeclarationHandler =

    /// Parse a single tile string into a Tile
    let private parseTile (tileStr: string) : Result<Tile.Tile, string> =
        match Tile.tryParseFromString tileStr with
        | Ok tile -> Ok tile
        | Error _ -> Error $"Invalid tile: {tileStr}"

    /// Validate tile count (must be exactly 13 for reach)
    let private validateTileCount (tiles: string list) : Result<string list, string> =
        match List.length tiles with
        | 13 -> Ok tiles
        | n -> Error $"Reach declaration requires exactly 13 tiles, but got {n}"

    /// Parse all tile strings
    let private parseTiles (tileStrings: string list) : Result<Tile.Tile list, string> =
        tileStrings
        |> List.map parseTile
        |> List.fold
            (fun acc result ->
                match acc, result with
                | Ok tiles, Ok tile -> Ok(tile :: tiles)
                | Error e, _ -> Error e
                | Ok _, Error e -> Error e)
            (Ok [])
        |> Result.map List.rev

    /// Validate no more than 4 of the same tile
    let private validateDuplicateLimit (tiles: Tile.Tile list) : Result<Tile.Tile list, string> =
        tiles
        |> List.countBy id
        |> List.tryFind (fun (_, count) -> count > 4)
        |> function
            | Some(tile, count) -> Error $"同じ牌は4枚までです。{Tile.toString tile}が{count}枚あります"
            | None -> Ok tiles

    /// Create a hand from tiles (13 tiles for reach)
    let private createHand (tiles: Tile.Tile list) : Result<Hand.Hand, string> =
        // For reach declaration, we need to create a Hand from 13 tiles
        // Use Waiting state which represents 13 tiles
        Ok(Hand.Waiting tiles)

    /// Convert ApplicationServices response to API response
    let private toApiResponse (response: ReachDeclarationService.ReachDeclarationResponse) : DeclareReachResponse =
        { success = response.success
          reachType = response.reachType
          error = response.error }

    /// Parse and validate a reach declaration request
    let private parseAndValidateRequest (request: DeclareReachRequest) : Result<Hand.Hand * int * int * bool, string> =
        request.tiles
        |> validateTileCount
        |> Result.bind parseTiles
        |> Result.bind validateDuplicateLimit
        |> Result.bind createHand
        |> Result.map (fun hand -> (hand, request.score, request.turn, request.hasReached))

    /// Full pipeline: parse request, declare reach, return response
    let handleDeclareReach (request: DeclareReachRequest) : Result<DeclareReachResponse, string> =
        parseAndValidateRequest request
        |> Result.map (fun (hand, score, turn, hasReached) ->
            ReachDeclarationService.declareReach hand score turn hasReached)
        |> Result.map toApiResponse
