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
