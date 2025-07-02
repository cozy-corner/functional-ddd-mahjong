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

    // ヘルパー関数: ソート済みリストから最初の順子を探す
    let private tryFindSequence sortedTiles =
        match sortedTiles with
        | [] -> None
        | t1 :: rest ->
            rest
            |> List.tryPick (fun t2 ->
                rest
                |> List.tryPick (fun t3 ->
                    if t1 <> t2 && t2 <> t3 then
                        match Meld.tryCreateSequence [ t1; t2; t3 ] with
                        | Ok seq ->
                            let remaining =
                                sortedTiles
                                |> removeItems t1 1
                                |> removeItems t2 1
                                |> removeItems t3 1

                            Some(seq, remaining)
                        | Error _ -> None
                    else
                        None))

    // シンプルな貪欲法で4面子を探す
    let private tryFindFourMelds tiles =
        let rec loop remaining foundMelds =
            if List.length foundMelds = 4 then
                if List.isEmpty remaining then
                    Some foundMelds
                else
                    None // 余り牌がある
            else
                // まず刻子を探す
                match tryFindTriplet remaining with
                | Some(triplet, newRemaining) -> loop newRemaining (triplet :: foundMelds)
                | None ->
                    // 次に順子を探す
                    let sorted =
                        List.sortWith Tile.compare remaining

                    match tryFindSequence sorted with
                    | Some(seq, newRemaining) -> loop newRemaining (seq :: foundMelds)
                    | None -> None // 面子が作れない

        loop tiles []

    // 内部実装: 14牌を4面子1雀頭に分解
    let tryDecomposeInternal tiles =
        // 各雀頭候補で分解を試みる
        findPairCandidates tiles
        |> List.tryPick (fun pairTile ->
            match Pair.tryCreatePair [ pairTile; pairTile ] with
            | Ok pair ->
                let remaining = removeItems pairTile 2 tiles

                match tryFindFourMelds remaining with
                | Some melds -> Some(melds, pair)
                | None -> None
            | Error _ -> None)
