namespace FunctionalDddMahjong.Domain

// テンパイ判定モジュール
module TenpaiAnalyzer =
    open Tile
    open Meld
    open Pair

    // テンパイのパターンを表現する型
    type TenpaiPattern =
        | FourMeldsWait of Meld list * Tile // 4面子完成の単騎待ち
        | ThreeMeldsOnePairWait of Meld list * Pair * IncompletePattern // 3面子1雀頭完成

    and IncompletePattern =
        | Ryanmen of Tile * Tile * WaitingTiles // 両面待ち (23 → 14待ち)
        | Kanchan of Tile * Tile * Tile // 嵌張待ち (13 → 2待ち)
        | Penchan of Tile * Tile * Tile // 辺張待ち (12 → 3待ち, 89 → 7待ち)
        | Shanpon of Tile * Tile // 双碰待ち (11と22 → どちらかが雀頭)

    and WaitingTiles = Tile list

    // 内部ヘルパー: リストから指定要素を削除
    let private removeItems item count list =
        let rec remove n acc =
            function
            | [] -> List.rev acc
            | h :: t ->
                if h = item && n > 0 then
                    remove (n - 1) acc t
                else
                    remove n (h :: acc) t

        remove count [] list

    // 内部ヘルパー: NumberValueの次の値を取得
    let private getNextNumber = function
        | One -> Some Two
        | Two -> Some Three
        | Three -> Some Four
        | Four -> Some Five
        | Five -> Some Six
        | Six -> Some Seven
        | Seven -> Some Eight
        | Eight -> Some Nine
        | Nine -> None

    // 内部ヘルパー: NumberValueの前の値を取得
    let private getPrevNumber = function
        | One -> None
        | Two -> Some One
        | Three -> Some Two
        | Four -> Some Three
        | Five -> Some Four
        | Six -> Some Five
        | Seven -> Some Six
        | Eight -> Some Seven
        | Nine -> Some Eight

    // 内部ヘルパー: 数牌の次の牌を取得（デバッグ用に一時的にpublic）
    let getNextTile tile =
        match getValue tile with
        | Character n -> getNextNumber n |> Option.map (Character >> create)
        | Circle n -> getNextNumber n |> Option.map (Circle >> create)
        | Bamboo n -> getNextNumber n |> Option.map (Bamboo >> create)
        | Honor _ -> None

    // 内部ヘルパー: 数牌の前の牌を取得（デバッグ用に一時的にpublic）
    let getPrevTile tile =
        match getValue tile with
        | Character n -> getPrevNumber n |> Option.map (Character >> create)
        | Circle n -> getPrevNumber n |> Option.map (Circle >> create)
        | Bamboo n -> getPrevNumber n |> Option.map (Bamboo >> create)
        | Honor _ -> None

    // 内部ヘルパー: 2枚の牌から待ち牌を判定（デバッグ用に一時的にpublic）
    let analyzeIncompletePair tiles =
        match List.sortWith Tile.compare tiles with
        | [ t1; t2 ] when t1 = t2 ->
            // 対子は双碰待ちの一部となる
            Some(Shanpon(t1, t2))
        | [ t1; t2 ] ->
            match getNextTile t1 with
            | Some next when next = t2 ->
                // 連続する2枚
                match getPrevTile t1, getNextTile t2 with
                | Some prev, Some next ->
                    // 両面待ち (23 → 14待ち)
                    Some(Ryanmen(t1, t2, [ prev; next ]))
                | Some prev, None ->
                    // 辺張待ち (89 → 7待ち)
                    Some(Penchan(t1, t2, prev))
                | None, Some next ->
                    // 辺張待ち (12 → 3待ち)
                    Some(Penchan(t1, t2, next))
                | None, None -> None
            | _ ->
                // 嵌張待ちの可能性をチェック
                match getNextTile t1 with
                | Some next1 ->
                    match getNextTile next1 with
                    | Some next2 when next2 = t2 ->
                        // 嵌張待ち (13 → 2待ち)
                        Some(Kanchan(t1, t2, next1))
                    | _ -> None
                | _ -> None
        | _ -> None

    // 内部ヘルパー: 牌のリストから雀頭候補を探す
    let private findPairCandidates tiles =
        tiles
        |> List.groupBy id
        |> List.filter (fun (_, group) -> List.length group >= 2)
        |> List.map fst



    // 13枚の手牌からテンパイパターンを分析
    let private analyzeTenpaiPatterns tiles =
        if List.length tiles <> 13 then
            []
        else
            let patterns = ResizeArray<TenpaiPattern>()

            // パターン1: 4面子完成の単騎待ち
            let fourMeldPatterns = MeldDecomposition.tryFindNMelds 4 tiles

            for (melds, remaining) in fourMeldPatterns do
                match remaining with
                | [ single ] -> patterns.Add(FourMeldsWait(melds, single))
                | _ -> ()

            // パターン2: 3面子1雀頭完成
            let pairCandidates = findPairCandidates tiles

            for pairTile in pairCandidates do
                match Pair.tryCreatePair [ pairTile; pairTile ] with
                | Ok pair ->
                    let tilesWithoutPair = removeItems pairTile 2 tiles
                    let threeMeldPatterns = MeldDecomposition.tryFindNMelds 3 tilesWithoutPair

                    for (melds, remaining) in threeMeldPatterns do
                        match remaining with
                        | [ _; _ ] as twoTiles ->
                            match analyzeIncompletePair twoTiles with
                            | Some incomplete ->
                                patterns.Add(ThreeMeldsOnePairWait(melds, pair, incomplete))
                            | None -> ()
                        | _ -> ()
                | Error _ -> ()

            patterns |> List.ofSeq

    // テンパイパターンから待ち牌を取得
    let private getWaitingTilesFromPattern pattern =
        match pattern with
        | FourMeldsWait(_, waitTile) -> [ waitTile ]
        | ThreeMeldsOnePairWait(_, _, incomplete) ->
            match incomplete with
            | Ryanmen(_, _, waitingTiles) -> waitingTiles
            | Kanchan(_, _, waitTile) -> [ waitTile ]
            | Penchan(_, _, waitTile) -> [ waitTile ]
            | Shanpon(t1, t2) -> [ t1; t2 ]

    // 13枚の手牌がテンパイかどうかを判定
    let isTenpai tiles =
        let patterns = analyzeTenpaiPatterns tiles
        not (List.isEmpty patterns)

    // 13枚の手牌から待ち牌のリストを取得
    let getWaitingTiles tiles =
        let patterns = analyzeTenpaiPatterns tiles

        patterns
        |> List.collect getWaitingTilesFromPattern
        |> List.distinct
        |> List.sortWith Tile.compare