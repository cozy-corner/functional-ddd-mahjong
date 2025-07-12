namespace FunctionalDddMahjong.Domain

// 役の分析と合成を行うモジュール
module YakuAnalyzer =
    open Hand
    open Yaku

    // 役判定結果の型
    type YakuAnalysisResult =
        { Yaku: Yaku.Yaku list // 成立した役のリスト
          TotalHan: int } // 合計翻数

    // 和了手牌の検証（Railway-Oriented Programming の最初のステップ）
    let validateWinningHand (hand: Hand.Hand) : Result<Hand.WinningHand, Yaku.YakuError> =
        if Hand.isWinningHand hand then
            match Hand.tryCreateWinningHand hand with
            | Some winningHand -> Ok winningHand
            | None -> Error(Yaku.YakuError.InvalidHandState "和了形の手牌を作成できませんでした")
        else
            Error(Yaku.YakuError.InvalidHandState "和了していない手牌です")

    // 和了手牌から全ての役を分析
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
        let detectedYaku =
            yakuCheckers
            |> List.choose (fun checker -> checker winningHand)

        // 合計翻数を計算
        let totalHan =
            detectedYaku |> List.map Yaku.getHan |> List.sum

        { Yaku = detectedYaku
          TotalHan = totalHan }

    // メイン分析関数（Railway-Oriented Programming）
    let analyzeYaku (hand: Hand.Hand) : Result<YakuAnalysisResult, Yaku.YakuError> =
        validateWinningHand hand
        |> Result.map analyzeAllYaku

    // 役なしの場合の判定
    let hasNoYaku (result: YakuAnalysisResult) : bool = result.Yaku.IsEmpty
