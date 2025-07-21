# Phase 7.1 PR2: 和了判定API - 入力変換とバリデーション

## 学習内容

### Railway-Oriented Programming の実践

#### Before: ネストが深い手続き的なコード
```fsharp
let parseAndValidateRequest (request: CheckWinningHandRequest) =
    if tileCount <> 14 then
        Error "..."
    else
        let parseResults = ...
        let errors = ...
        if not (List.isEmpty errors) then
            Error ...
        else
            let tiles = ...
            let tileCounts = ...
            match tileCounts with
            | ... -> Error ...
            | [] -> 
                match tiles with
                | [] -> Error ...
                | _ -> Ok ...
```

#### After: 関数合成による実装
```fsharp
let parseAndValidateRequest (request: CheckWinningHandRequest) =
    request.tiles
    |> validateTileCount
    |> Result.bind parseTiles
    |> Result.bind validateDuplicateLimit
    |> Result.bind createHand
```

### List.fold を使った fail-fast パターン

```fsharp
let parseTiles (tileStrings: string list) =
    tileStrings
    |> List.map parseTile
    |> List.fold
        (fun acc result ->
            match acc, result with
            | Ok tiles, Ok tile -> Ok(tile :: tiles)  // 成功継続
            | Error e, _ -> Error e                    // エラー保持
            | Ok _, Error e -> Error e)                // エラー発生
        (Ok [])
    |> Result.map List.rev
```

**ポイント:**
- 最初のエラーで処理を停止（fail-fast）
- 効率的な先頭追加（O(1)）を使用
- 最後に順序を反転

### 関数分解による責務の明確化

1. **validateTileCount**: 枚数検証のみ
2. **parseTiles**: 文字列→Tile変換のみ
3. **validateDuplicateLimit**: 重複チェックのみ
4. **createHand**: Hand作成のみ

各関数が単一責任を持ち、合成可能。

## 設計判断

### なぜ fail-fast を選択したか
- APIユーザーは最初のエラーを修正することから始める
- 全エラー収集（Validation.sequence）は過剰な情報になりがち
- シンプルな実装で十分な場合が多い

### なぜ List.fold を使ったか
- 再帰よりもスタックセーフ
- パターンマッチで全ケースを網羅
- F#の標準的なイディオム

## 関連ファイル
- `/src/FunctionalDddMahjong.Api/CheckWinningHandHandler.fs`
- `/tests/FunctionalDddMahjong.Api.Tests/CheckWinningHandHandlerTests.fs`