namespace FunctionalDddMahjong.Domain

/// 巡目を表現するValue Object（1-18の制約）
type Turn = private Turn of int

module Turn =
    let create turn =
        if turn >= 1 && turn <= 18 then
            Ok(Turn turn)
        else
            Error "Turn must be between 1 and 18"

    let value (Turn turn) = turn

/// 点数を表現するValue Object（0以上の制約）
type Score = private Score of int

module Score =
    let create score =
        if score >= 0 then
            Ok(Score score)
        else
            Error "Score must be non-negative"

    let value (Score score) = score

/// プレイヤーのリーチ状態を表現する型
type ReachStatus =
    | NotReached // リーチしていない
    | AlreadyReached // 既にリーチ済み

/// リーチ宣言に必要なゲーム状況を表現する型
type ReachContext =
    { PlayerScore: Score // プレイヤーの現在点数
      CurrentTurn: Turn // 現在の巡目
      ReachStatus: ReachStatus } // 現在のリーチ状態

/// リーチ宣言時のドメイン特化エラーを表現する型
type ReachError =
    | NotTenpai // テンパイしていない
    | InsufficientScore // 点数不足（1000点未満）
    | TooLateInGame // ゲーム終盤でのリーチ制限（16巡目以降）
    | AlreadyReached // 既にリーチ済み

/// リーチの種類を表現する型
type ReachResult =
    | Reach // 通常リーチ（1翻）
    | DoubleReach // ダブルリーチ（2翻）

/// リーチ宣言の結果を表現する型（標準Result型を使用）
type ReachDeclaration = Result<ReachResult * Score, ReachError>

/// リーチ宣言のバリデーション関数モジュール
module ReachValidation =
    open TenpaiAnalyzer

    /// テンパイ状態をチェックする関数
    let checkTenpai (hand: Hand.Hand) : Result<unit, ReachError> =
        let tiles = Hand.getTiles hand

        if TenpaiAnalyzer.isTenpai tiles then
            Ok()
        else
            Error NotTenpai

    /// 点数をチェックする関数（1000点以上必要）
    let checkScore (context: ReachContext) : Result<unit, ReachError> =
        let currentScore =
            Score.value context.PlayerScore

        if currentScore >= 1000 then
            Ok()
        else
            Error InsufficientScore

    /// ゲーム段階をチェックする関数（16巡目以降はリーチ不可）
    let checkGameStage (context: ReachContext) : Result<unit, ReachError> =
        let currentTurn =
            Turn.value context.CurrentTurn

        if currentTurn < 16 then
            Ok()
        else
            Error TooLateInGame

    /// リーチ状態をチェックする関数（既にリーチ済みは不可）
    let checkReachStatus (context: ReachContext) : Result<unit, ReachError> =
        match context.ReachStatus with
        | ReachStatus.NotReached -> Ok()
        | ReachStatus.AlreadyReached -> Error ReachError.AlreadyReached

/// リーチ宣言のメイン実行モジュール
module ReachDeclaration =
    open ReachValidation

    /// バリデーション結果をリーチ結果に変換する関数
    let private determineReachResult (context: ReachContext) : ReachResult * Score =
        let currentTurn =
            Turn.value context.CurrentTurn

        let currentScore =
            Score.value context.PlayerScore

        let newScore =
            match Score.create (currentScore - 1000) with
            | Ok score -> score
            | Error msg ->
                // This should never happen since we validated currentScore >= 1000
                failwith $"Unexpected score calculation error: {msg}"

        if currentTurn = 1 then
            (DoubleReach, newScore) // 1巡目はダブルリーチ
        else
            (Reach, newScore) // それ以外は通常リーチ

    /// リーチ宣言のメイン関数（Railway-Oriented Programming）
    let declareReach (hand: Hand.Hand) (context: ReachContext) : ReachDeclaration =
        checkTenpai hand
        |> Result.bind (fun _ -> checkScore context)
        |> Result.bind (fun _ -> checkGameStage context)
        |> Result.bind (fun _ -> checkReachStatus context)
        |> Result.map (fun _ -> determineReachResult context)
