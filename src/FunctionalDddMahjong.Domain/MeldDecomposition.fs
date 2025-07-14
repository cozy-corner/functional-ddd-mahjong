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

    // 汎用的な組み合わせ生成（再帰）
    let rec private combinations n list =
        match n, list with
        | 0, _ -> [[]]
        | _, [] -> []
        | n, x::xs ->
            let withX = combinations (n-1) xs |> List.map (fun combo -> x::combo)
            let withoutX = combinations n xs
            withX @ withoutX

    // インデックス付き組み合わせ
    let private indexedCombinations n indexedList =
        indexedList
        |> List.map fst
        |> combinations n
        |> List.map (fun indices -> 
            indices |> List.map (fun i -> 
                indexedList |> List.find (fun (idx, _) -> idx = i)))

    // ヘルパー関数: ソート済みリストから全ての順子候補を探す
    let private findAllSequenceCandidates sortedTiles =
        let indexed = List.indexed sortedTiles
        
        indexed
        |> indexedCombinations 3
        |> List.choose (fun combination ->
            let indices, tiles = List.unzip combination
            match Meld.tryCreateSequence tiles with
            | Ok seq ->
                let remaining = 
                    indexed
                    |> List.filter (fun (i, _) -> not (List.contains i indices))
                    |> List.map snd
                Some(seq, remaining)
            | Error _ -> None)

    // バックトラッキングで指定数の面子パターンを探す（余り牌も返す）
    let private tryFindNMeldsInternal targetCount tiles =
        let rec backtrack remaining foundMelds allResults =
            match List.length foundMelds, List.length remaining with
            // 目標数の面子が見つかった場合
            | n, _ when n = targetCount -> (foundMelds, remaining) :: allResults
            // 面子がまだ足りないが残り牌で作れない場合
            | foundCount, remainingCount when foundCount < targetCount && remainingCount < (3 * (targetCount - foundCount)) -> allResults
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

    // 4面子を探す（従来の関数、互換性のため）
    let private tryFindAllFourMelds tiles = tryFindNMeldsInternal 4 tiles

    // 汎用: 指定された数の面子を探す（余り牌も返す）
    let tryFindNMelds targetCount tiles = tryFindNMeldsInternal targetCount tiles

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

                    // 各面子パターンと雀頭を組み合わせる（余り牌が0枚の場合のみ）
                    allMeldPatterns
                    |> List.choose (fun (melds, remaining) ->
                        if List.isEmpty remaining then
                            Some (melds, pair)
                        else
                            None)
                | Error _ -> [])

        // 重複除去: 正規化した結果で比較
        allPatterns
        |> List.distinctBy normalizeDecomposition
