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
        // Tanyao
        Assert.Equal("断么九", getName Tanyao)
        Assert.Equal(1, getHan Tanyao)
        Assert.Equal("All Simples", getEnglishName Tanyao)

        // Pinfu
        Assert.Equal("平和", getName Pinfu)
        Assert.Equal(1, getHan Pinfu)
        Assert.Equal("All Sequences", getEnglishName Pinfu)

        // Toitoi
        Assert.Equal("対々和", getName Toitoi)
        Assert.Equal(2, getHan Toitoi)
        Assert.Equal("All Triplets", getEnglishName Toitoi)

        // Honitsu
        Assert.Equal("混一色", getName Honitsu)
        Assert.Equal(3, getHan Honitsu)
        Assert.Equal("Half Flush", getEnglishName Honitsu)

        // Chinitsu
        Assert.Equal("清一色", getName Chinitsu)
        Assert.Equal(6, getHan Chinitsu)
        Assert.Equal("Full Flush", getEnglishName Chinitsu)

    [<Fact>]
    let ``tryCreateWinningHand should return None for waiting hand`` () =
        let tileStrings =
            [ "2m"
              "3m"
              "4m"
              "5m"
              "6m"
              "7m"
              "8m"
              "2p"
              "3p"
              "4p"
              "5p"
              "6p"
              "7p" ]

        let tiles = createTiles tileStrings
        let hand = Waiting tiles
        let result = tryCreateWinningHand hand

        match result with
        | None -> Assert.True(true)
        | Some _ -> Assert.True(false, "Should not create WinningHand from waiting hand")

    [<Theory>]
    [<InlineData("2m,3m,4m,5m,6m,7m,2p,3p,4p,5p,6p,7p,8s,8s")>] // タンヤオ成立手
    let ``checkTanyao should accept valid tanyao hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

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
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

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
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        let result = tryCreateWinningHand hand

        match result with
        | None -> Assert.True(true)
        | Some _ -> Assert.True(false, "Should not create WinningHand from non-winning hand")

    // ピンフのテストケース
    [<Theory>]
    [<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,2m,3m,4m,5p,5p")>] // 順子4つ + 数牌の雀頭
    [<InlineData("2m,3m,4m,3p,4p,5p,6p,7p,8p,4s,5s,6s,7s,7s")>] // 順子4つ + 数牌の雀頭（中張牌）
    [<InlineData("1p,2p,3p,4p,5p,6p,7p,8p,9p,1s,2s,3s,2m,2m")>] // 清一色の順子 + 数牌の雀頭
    let ``checkPinfu should accept valid pinfu hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkPinfu winningHand

            match result with
            | Some Pinfu -> Assert.True(true)
            | None -> Assert.True(false, "Should accept valid pinfu hand")
        | None -> Assert.True(false, "Should be a winning hand")

    [<Theory>]
    [<InlineData("1m,2m,3m,4p,5p,6p,7s,7s,7s,2m,3m,4m,5p,5p")>] // 刻子を含む
    [<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,2m,3m,4m,E,E")>] // 字牌の雀頭
    [<InlineData("2m,2m,2m,5p,5p,5p,8s,8s,8s,3m,3m,3m,7p,7p")>] // 全て刻子（トイトイ）
    let ``checkPinfu should reject invalid pinfu hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkPinfu winningHand

            match result with
            | None -> Assert.True(true)
            | Some _ -> Assert.True(false, "Should reject invalid pinfu hand")
        | None -> Assert.True(false, "Should be a winning hand")

    // トイトイのテストケース
    [<Theory>]
    [<InlineData("2m,2m,2m,5p,5p,5p,8s,8s,8s,3m,3m,3m,7p,7p")>] // 刻子4つ + 雀頭
    [<InlineData("1m,1m,1m,9p,9p,9p,E,E,E,S,S,S,W,W")>] // 么九牌の刻子も含む
    [<InlineData("4s,4s,4s,5s,5s,5s,6s,6s,6s,7s,7s,7s,8s,8s")>] // 清一色の刻子
    let ``checkToitoi should accept valid toitoi hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkToitoi winningHand

            match result with
            | Some Toitoi -> Assert.True(true)
            | None -> Assert.True(false, "Should accept valid toitoi hand")
        | None -> Assert.True(false, "Should be a winning hand")

    [<Theory>]
    [<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,2m,3m,4m,5p,5p")>] // ピンフ（全て順子）
    [<InlineData("1m,2m,3m,5p,5p,5p,7s,8s,9s,2m,3m,4m,3p,3p")>] // 順子3つ + 刻子1つ
    [<InlineData("2m,2m,2m,3m,4m,5m,6p,7p,8p,7s,8s,9s,E,E")>] // 刻子1つ + 順子3つ
    let ``checkToitoi should reject hand with sequences`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkToitoi winningHand

            match result with
            | None -> Assert.True(true)
            | Some _ -> Assert.True(false, "Should reject toitoi hand with sequences")
        | None -> Assert.True(false, "Should be a winning hand")

    // ホンイツのテストケース
    [<Theory>]
    [<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,E,E,E,S,S")>] // 萬子 + 字牌
    [<InlineData("2p,2p,2p,5p,5p,5p,8p,8p,8p,W,W,W,N,N")>] // 筒子の刻子 + 字牌
    [<InlineData("1s,2s,3s,4s,5s,6s,7s,8s,9s,1s,2s,3s,E,E")>] // 索子の順子 + 字牌
    [<InlineData("1m,1m,1m,2m,3m,4m,5m,6m,7m,E,E,E,N,N")>] // 萬子の混合 + 複数種の字牌
    let ``checkHonitsu should accept valid honitsu hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkHonitsu winningHand

            match result with
            | Some Honitsu -> Assert.True(true)
            | None -> Assert.True(false, "Should accept valid honitsu hand")
        | None -> Assert.True(false, "Should be a winning hand")

    [<Theory>]
    [<InlineData("1m,2m,3m,4p,5p,6p,7s,8s,9s,E,E,E,S,S")>] // 複数の数牌スート
    [<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,1m,2m,3m,4m,4m")>] // 数牌のみ（字牌なし）
    [<InlineData("E,E,E,S,S,S,W,W,W,N,N,N,H,H")>] // 字牌のみ
    let ``checkHonitsu should reject invalid honitsu hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkHonitsu winningHand

            match result with
            | None -> Assert.True(true)
            | Some _ -> Assert.True(false, "Should reject invalid honitsu hand")
        | None -> Assert.True(false, "Should be a winning hand")

    // チンイツのテストケース
    [<Theory>]
    [<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,1m,2m,3m,5m,5m")>] // 萬子のみ
    [<InlineData("1p,1p,1p,2p,3p,4p,5p,6p,7p,8p,9p,9p,9p,9p")>] // 筒子のみ（刻子含む）
    [<InlineData("2s,3s,4s,3s,4s,5s,4s,5s,6s,5s,6s,7s,8s,8s")>] // 索子のみ（重複順子）
    [<InlineData("1m,1m,1m,2m,2m,2m,3m,3m,3m,4m,4m,4m,5m,5m")>] // 萬子のみ（全て刻子）
    let ``checkChinitsu should accept valid chinitsu hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkChinitsu winningHand

            match result with
            | Some Chinitsu -> Assert.True(true)
            | None -> Assert.True(false, "Should accept valid chinitsu hand")
        | None -> Assert.True(false, "Should be a winning hand")

    [<Theory>]
    [<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,E,E,E,S,S")>] // 字牌を含む（ホンイツ）
    [<InlineData("1m,2m,3m,4p,5p,6p,7p,8p,9p,1s,2s,3s,5m,5m")>] // 複数の数牌スート
    [<InlineData("1m,2m,3m,4m,5m,6m,7m,8m,9m,1p,1p,1p,2s,2s")>] // 複数の数牌スート
    let ``checkChinitsu should reject invalid chinitsu hand`` (tileString: string) =
        let tileStrings =
            tileString.Split(',') |> Array.toList

        let hand =
            createReadyHand (createTiles tileStrings)

        match tryCreateWinningHand hand with
        | Some winningHand ->
            let result = checkChinitsu winningHand

            match result with
            | None -> Assert.True(true)
            | Some _ -> Assert.True(false, "Should reject invalid chinitsu hand")
        | None -> Assert.True(false, "Should be a winning hand")
