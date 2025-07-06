namespace FunctionalDddMahjong.Domain

// 内部実装モジュール - 面子分解のアルゴリズム
module internal MeldDecomposition =
    open Tile
    open Meld
    open Pair

    // ヘルパー関数: リストから指定された要素を指定個数だけ削除
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

    // ヘルパー関数: 牌のリストから雀頭候補を探す
    let private findPairCandidates tiles =
        tiles
        |> List.groupBy id
        |> List.filter (fun (_, group) -> List.length group >= 2)
        |> List.map fst

    // ヘルパー関数: 牌のリストから刻子を探して作成
    let private tryFindTriplet tiles =
        tiles
        |> List.groupBy id
        |> List.tryPick (fun (tile, group) ->
            if List.length group >= 3 then
                match Meld.tryCreateTriplet [ tile; tile; tile ] with
                | Ok triplet -> Some(triplet, removeItems tile 3 tiles)
                | Error _ -> None
            else
                None)

    // ヘルパー関数: ソート済みリストから全ての順子候補を探す
    let private findAllSequenceCandidates sortedTiles =
        match sortedTiles with
        | [] -> []
        | first :: rest ->
            let rec findSequences remainingForT2 acc =
                match remainingForT2 with
                | [] -> acc
                | t2 :: tailForT3 ->
                    let sequencesWithT2 =
                        tailForT3
                        |> List.choose (fun t3 ->
                            if first <> t2 && t2 <> t3 then
                                match Meld.tryCreateSequence [ first; t2; t3 ] with
                                | Ok seq ->
                                    let remaining =
                                        sortedTiles
                                        |> removeItems first 1
                                        |> removeItems t2 1
                                        |> removeItems t3 1

                                    Some(seq, remaining)
                                | Error _ -> None
                            else
                                None)

                    findSequences tailForT3 (sequencesWithT2 @ acc)

            findSequences rest [] |> List.rev

    // バックトラッキングで全ての4面子パターンを探す
    let private tryFindAllFourMelds tiles =
        let rec backtrack remaining foundMelds allResults =
            match List.length foundMelds, List.length remaining with
            // 4面子見つかった場合
            | 4, 0 -> foundMelds :: allResults // 完成パターンを追加
            | 4, _ -> allResults // 余り牌がある
            // 残り牌が足りない場合
            | _, n when n < 3 -> allResults
            // 面子を探す
            | _ ->
                // 現在の牌をソート
                let sorted =
                    List.sortWith Tile.compare remaining

                // 可能な面子候補を生成
                let tripletCandidates =
                    match tryFindTriplet sorted with
                    | Some(triplet, rest) -> [ (triplet, rest) ]
                    | None -> []

                let sequenceCandidates =
                    findAllSequenceCandidates sorted

                let candidates =
                    tripletCandidates @ sequenceCandidates

                // 各候補を試してバックトラック（全ての結果を収集）
                candidates
                |> List.fold
                    (fun accResults (meld, newRemaining) ->
                        let newResults =
                            backtrack newRemaining (meld :: foundMelds) []

                        accResults @ newResults)
                    []

        backtrack tiles [] []

    // ヘルパー関数: 分解結果の正規化（比較用）
    let private normalizeDecomposition (melds, pair) =
        let sortedMelds =
            melds
            |> List.map (fun meld ->
                match getMeldValue meld with
                | Sequence(t1, t2, t3) -> [ t1; t2; t3 ]
                | Triplet(t1, t2, t3) -> [ t1; t2; t3 ]
                |> List.sortWith Tile.compare)
            |> List.sortWith (fun a b ->
                match a, b with
                | h1 :: _, h2 :: _ -> Tile.compare h1 h2
                | _ -> 0)

        let sortedPair =
            getPairTiles pair |> List.sortWith Tile.compare

        (sortedMelds, sortedPair)

    // 内部実装: 14牌を全ての4面子1雀頭パターンに分解
    let tryDecomposeAllInternal tiles =
        // 各雀頭候補で全ての分解パターンを収集
        let allPatterns =
            findPairCandidates tiles
            |> List.collect (fun pairTile ->
                match Pair.tryCreatePair [ pairTile; pairTile ] with
                | Ok pair ->
                    let remaining = removeItems pairTile 2 tiles

                    let allMeldPatterns =
                        tryFindAllFourMelds remaining

                    // 各面子パターンと雀頭を組み合わせる
                    allMeldPatterns
                    |> List.map (fun melds -> (melds, pair))
                | Error _ -> [])

        // 重複除去: 正規化した結果で比較
        allPatterns
        |> List.distinctBy normalizeDecomposition
