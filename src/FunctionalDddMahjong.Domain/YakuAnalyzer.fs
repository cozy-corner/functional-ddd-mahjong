namespace FunctionalDddMahjong.Domain

// 役の分析と合成を行うモジュール
module YakuAnalyzer =
    open Hand
    open Yaku

    // 役判定結果の型
    type YakuAnalysisResult =
        { Yaku: Yaku.Yaku list // 成立した役のリスト
          TotalHan: int } // 合計翻数

    // 相互排他関係の定義（将来の拡張のため、複数グループを定義可能）
    let private mutuallyExclusivePairs =
        [
          // 一盃口とトイトイは相互排他（同じ順子2つ vs 全て刻子）
          Set.ofList [ Yaku.Iipeikou; Yaku.Toitoi ]
          // 将来の例: 二盃口とトイトイも相互排他
          // Set.ofList [ Yaku.Ryanpeikou; Yaku.Toitoi ]
          ]

    // 相互排他関係を解決して最終的な役リストを取得
    let private resolveExclusivity (allDetectedYaku: Yaku.Yaku list) : Yaku.Yaku list =
        // 各相互排他グループで競合する役を見つけて、最高翻数の役を選択
        let resolvedYaku =
            mutuallyExclusivePairs
            |> List.fold
                (fun currentYaku exclusivePair ->
                    let conflictingYaku =
                        currentYaku
                        |> List.filter (fun yaku -> Set.contains yaku exclusivePair)

                    match conflictingYaku with
                    | [] -> currentYaku // 競合なし
                    | [ _ ] -> currentYaku // 1つだけなら競合なし
                    | multiple ->
                        // 複数ある場合は最高翻数の役を選択
                        let bestYaku =
                            multiple |> List.maxBy Yaku.getHan

                        // 競合する役を除去し、最高翻数の役のみ残す
                        currentYaku
                        |> List.filter (fun yaku -> not (Set.contains yaku exclusivePair))
                        |> fun remaining -> bestYaku :: remaining)
                allDetectedYaku

        resolvedYaku

    // 和了手牌の検証（Railway-Oriented Programming の最初のステップ）
    let validateWinningHand (hand: Hand.Hand) : Result<Hand.WinningHand, Yaku.YakuError> =
        if Hand.isWinningHand hand then
            match Hand.tryCreateWinningHand hand with
            | Some winningHand -> Ok winningHand
            | None -> Error(Yaku.YakuError.InvalidHandState "和了形の手牌を作成できませんでした")
        else
            Error(Yaku.YakuError.InvalidHandState "和了していない手牌です")

    // 和了手牌から全ての役を分析（高点法適用）
    let analyzeAllYaku (winningHand: Hand.WinningHand) : YakuAnalysisResult =
        // 全ての役判定関数を並列実行
        let yakuCheckers =
            [ Yaku.checkTanyao
              Yaku.checkPinfu
              Yaku.checkToitoi
              Yaku.checkHonitsu
              Yaku.checkChinitsu
              Yaku.checkIipeikou ]

        // Option型の結果をフィルタリングして成立した役のみ取得
        let allDetectedYaku =
            yakuCheckers
            |> List.choose (fun checker -> checker winningHand)

        // 役なしの場合
        if List.isEmpty allDetectedYaku then
            { Yaku = []; TotalHan = 0 }
        else
            // 相互排他関係を解決して最終的な役を決定
            let finalYaku =
                resolveExclusivity allDetectedYaku

            let totalHan =
                finalYaku |> List.map Yaku.getHan |> List.sum

            { Yaku = finalYaku
              TotalHan = totalHan }

    // メイン分析関数（Railway-Oriented Programming）
    let analyzeYaku (hand: Hand.Hand) : Result<YakuAnalysisResult, Yaku.YakuError> =
        validateWinningHand hand
        |> Result.map analyzeAllYaku

    // 役なしの場合の判定
    let hasNoYaku (result: YakuAnalysisResult) : bool = result.Yaku.IsEmpty
