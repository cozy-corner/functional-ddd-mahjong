namespace FunctionalDddMahjong.ApplicationServices

open FunctionalDddMahjong.Domain
open FunctionalDddMahjong.Workflows

module ReachDeclarationService =

    /// Response from reach declaration
    type ReachDeclarationResponse =
        {
            /// Whether the reach declaration was successful
            success: bool

            /// Type of reach if successful ("Reach" or "DoubleReach")
            reachType: string option

            /// Error message if reach declaration failed
            error: string option
        }

    /// Converts reach errors to user-friendly messages
    let private errorToMessage (error: ReachError) : string =
        match error with
        | NotTenpai -> "手牌が聴牌していません"
        | InsufficientScore -> "点数が不足しています（1000点必要）"
        | TooLateInGame -> "ゲーム終盤のためリーチできません（15巡目まで）"
        | AlreadyReached -> "すでにリーチしています"

    /// Converts reach result to response
    let private resultToResponse (result: ReachResult) : ReachDeclarationResponse =
        match result with
        | Reach ->
            { success = true
              reachType = Some "Reach"
              error = None }
        | DoubleReach ->
            { success = true
              reachType = Some "DoubleReach"
              error = None }

    /// Declares reach for a given hand
    let declareReach (hand: Hand.Hand) (score: int) (turn: int) (hasReached: bool) : ReachDeclarationResponse =
        // Create Score and Turn value objects
        let scoreResult = Score.create score
        let turnResult = Turn.create turn

        match scoreResult, turnResult with
        | Error msg, _
        | _, Error msg ->
            { success = false
              reachType = None
              error = Some msg }
        | Ok scoreValue, Ok turnValue ->
            // Create reach context
            let reachStatus =
                if hasReached then
                    ReachStatus.AlreadyReached
                else
                    ReachStatus.NotReached

            let context =
                { PlayerScore = scoreValue
                  CurrentTurn = turnValue
                  ReachStatus = reachStatus }

            // Declare reach
            let reachResult =
                ReachDeclaration.declareReach hand context

            match reachResult with
            | Ok(result, _) -> resultToResponse result
            | Error error ->
                { success = false
                  reachType = None
                  error = Some(errorToMessage error) }
