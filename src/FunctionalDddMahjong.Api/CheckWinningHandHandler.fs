namespace FunctionalDddMahjong.Api

open FunctionalDddMahjong.Domain

/// Handles the conversion and validation of winning hand check requests
module CheckWinningHandHandler =

    /// Parse a single tile string into a Tile
    /// This is a wrapper around Tile.tryParseFromString that provides API-specific error messages
    let parseTile (tileStr: string) : Result<Tile.Tile, string> =
        match Tile.tryParseFromString tileStr with
        | Ok tile -> Ok tile
        | Error _ -> Error $"Invalid tile: {tileStr}"

    /// Validate tile count (must be exactly 14)
    let private validateTileCount (tiles: string list) : Result<string list, string> =
        match List.length tiles with
        | 14 -> Ok tiles
        | n -> Error $"Hand must have exactly 14 tiles, but got {n}"

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

    /// Create a ready hand from tiles
    let private createHand (tiles: Tile.Tile list) : Result<Hand.Hand, string> = Ok(Hand.Ready tiles)

    /// Parse and validate a winning hand check request
    /// Converts string list to Hand, ensuring exactly 14 tiles
    let parseAndValidateRequest (request: CheckWinningHandRequest) : Result<Hand.Hand, string> =
        request.tiles
        |> validateTileCount
        |> Result.bind parseTiles
        |> Result.bind validateDuplicateLimit
        |> Result.bind createHand
