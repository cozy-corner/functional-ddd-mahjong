namespace FunctionalDddMahjong.Domain.Tests

open Xunit
open FunctionalDddMahjong.Domain.Tile
open FunctionalDddMahjong.Domain.Hand
open FunctionalDddMahjong.Domain.Yaku

module YakuTests =

    // テスト用のヘルパー関数: 文字列のリストから牌を作成
    let private createTiles tileStrings =
        tileStrings
        |> List.choose (fun str ->
            match tryParseFromString str with
            | Ok tile -> Some tile
            | Error _ -> None)

    // テスト用のヘルパー関数: 14牌の手牌を作成
    let private createReadyHand tiles =
        match tiles with
        | [] -> failwith "Empty tile list"
        | _ when List.length tiles <> 14 -> failwith $"Expected 14 tiles, got {List.length tiles}"
        | _ ->
            // 最初の13牌で手牌を作り、14牌目をツモる
            let first13 = tiles |> List.take 13
            let lastTile = tiles |> List.item 13

            match tryCreateFromDeal first13 with
            | Ok waitingHand -> draw lastTile waitingHand
            | Error e -> failwith $"Failed to create hand: {e}"

    [<Fact>]
    let ``Yaku types should have correct properties`` () =
        Assert.Equal("断么九", getName Tanyao)
        Assert.Equal(1, getHan Tanyao)
        Assert.Equal("All Simples", getEnglishName Tanyao)

    [<Fact>]
    let ``tryCreateWinningHand should return None for waiting hand`` () =
        let tileStrings = [
            "2m"; "3m"; "4m"; "5m"; "6m"; "7m"; "8m"
            "2p"; "3p"; "4p"; "5p"; "6p"; "7p"
        ]
        let tiles = createTiles tileStrings
        let hand = Waiting tiles
        let result = tryCreateWinningHand hand
        
        match result with
        | None -> Assert.True(true)
        | Some _ -> Assert.True(false, "Should not create WinningHand from waiting hand")

    [<Theory>]
    [<InlineData("2m,3m,4m,5m,6m,7m,2p,3p,4p,5p,6p,7p,8s,8s")>] // タンヤオ成立手
    let ``checkTanyao should accept valid tanyao hand`` (tileString: string) =
        let tileStrings = tileString.Split(',') |> Array.toList
        let hand = createReadyHand (createTiles tileStrings)
        
        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkTanyao winningHand
            match result with
            | Some Tanyao -> Assert.True(true)
            | None -> Assert.True(false, "Should accept valid tanyao hand")
        | None -> Assert.True(false, "Should be a winning hand")

    [<Theory>]
    [<InlineData("1m,2m,3m,5m,6m,7m,2p,3p,4p,5p,6p,7p,8s,8s")>] // 1m（么九牌）を含む
    [<InlineData("2m,3m,4m,5m,6m,7m,2p,3p,4p,5p,6p,7p,E,E")>] // E（字牌）を含む
    [<InlineData("2m,3m,4m,7m,8m,9m,2p,3p,4p,5p,6p,7p,8s,8s")>] // 9m（么九牌）を含む
    let ``checkTanyao should reject hand with terminal or honor tiles`` (tileString: string) =
        let tileStrings = tileString.Split(',') |> Array.toList
        let hand = createReadyHand (createTiles tileStrings)
        
        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkTanyao winningHand
            match result with
            | None -> Assert.True(true)
            | Some _ -> Assert.True(false, "Should reject hand with terminal or honor tiles")
        | None -> Assert.True(false, "Should be a winning hand")

    [<Theory>]
    [<InlineData("2m,3m,4m,5m,6m,7m,2p,3p,4p,5p,6p,7p,8p,9p")>] // 和了形にならない手牌
    let ``tryCreateWinningHand should return None for non-winning hand`` (tileString: string) =
        let tileStrings = tileString.Split(',') |> Array.toList
        let hand = createReadyHand (createTiles tileStrings)
        let result = tryCreateWinningHand hand
        
        match result with
        | None -> Assert.True(true)
        | Some _ -> Assert.True(false, "Should not create WinningHand from non-winning hand")