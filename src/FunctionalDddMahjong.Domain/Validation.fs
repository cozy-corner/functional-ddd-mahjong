namespace FunctionalDddMahjong.Domain

/// エラー集約バリデーションのためのユーティリティモジュール
module Validation =

    /// 単一エラーから複数エラーリストへの変換
    let singleton error = [ error ]

    /// Result<'a, 'e> list -> Result<'a list, 'e list>
    /// 全てのResultを評価し、成功時は値のリスト、失敗時は全エラーのリストを返す
    let sequence (results: Result<'a, 'e> list) : Result<'a list, 'e list> =
        let folder acc result =
            match acc, result with
            | Ok values, Ok value -> Ok(values @ [ value ])
            | Error errors, Error e -> Error(errors @ [ e ])
            | Ok _, Error e -> Error [ e ]
            | Error errors, Ok _ -> Error errors

        let initial = Ok []
        results |> List.fold folder initial

    /// ('a -> Result<'b, 'e>) -> 'a list -> Result<'b list, 'e list>
    /// リストの各要素に関数を適用し、全ての結果を集約する
    let traverse (f: 'a -> Result<'b, 'e>) (list: 'a list) : Result<'b list, 'e list> = list |> List.map f |> sequence

    /// Applicativeスタイルの適用演算子
    /// Result<('a -> 'b), 'e list> -> Result<'a, 'e list> -> Result<'b, 'e list>
    let apply (fResult: Result<('a -> 'b), 'e list>) (xResult: Result<'a, 'e list>) : Result<'b, 'e list> =
        match fResult, xResult with
        | Ok f, Ok x -> Ok(f x)
        | Error e1, Error e2 -> Error(e1 @ e2)
        | Error e, _ -> Error e
        | _, Error e -> Error e

    /// Applicative演算子（中置記法用）
    let (<*>) = apply

    /// 単一エラーResultをエラーリストResultに変換
    let liftError (result: Result<'a, 'e>) : Result<'a, 'e list> = result |> Result.mapError singleton

    /// 独立したバリデーションを並列実行し、全てのエラーを集約する
    /// validations: 実行するバリデーション関数のリスト
    /// すべて成功時: Ok ()
    /// 失敗時: Error [全てのエラー]
    let validateAll (validations: (unit -> Result<unit, 'e>) list) : Result<unit, 'e list> =
        validations
        |> List.map (fun validation -> validation () |> liftError)
        |> sequence
        |> Result.map (fun _ -> ())
        |> Result.mapError List.concat
