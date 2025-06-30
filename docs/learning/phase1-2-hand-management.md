# フェーズ1.2 Hand管理 - 学習成果

## 概要

麻雀の手牌を管理する型安全なドメインモデルを実装し、判別共用体による状態管理と関数型DDDの進歩的概念を学習しました。

## 実装内容

### 1. 判別共用体による型安全な状態管理

```fsharp
// 手牌を表現する型（判別共用体）
// 型レベルで13牌と14牌を区別
type Hand =
    | Waiting of Tile list // 13牌（ツモ待ち）
    | Ready of Tile list   // 14牌（打牌可能）
```

**学習ポイント**: 
- 麻雀の自然な状態遷移を型で表現
- 不正な状態（15牌など）をコンパイル時に排除
- ビジネスルール（13牌でツモ、14牌で打牌）が型定義に直接反映

### 2. 型安全な操作関数

```fsharp
// ツモ（13牌→14牌、型安全）
let draw tile (Waiting tiles) = Ready(tile :: tiles)

// 打牌（14牌→13牌、型安全）
let discard tile (Ready tiles) =
    (* 牌の検索と削除処理 *)
```

**学習ポイント**:
- **コンパイル時安全性**: 13牌の手牌に`discard`は呼べない
- **関数シグネチャの明確性**: 期待する状態が型で表現
- **エラーの削減**: `AlreadyDrawn`、`NothingToDiscard`エラーが不要

### 3. パターンマッチングによる共通操作の実装

```fsharp
// 共通操作：手牌の内容を取得
let getTiles =
    function
    | Waiting tiles
    | Ready tiles -> tiles

// 共通操作：手牌をソート
let sort =
    function
    | Waiting tiles -> Waiting(List.sortWith Tile.compare tiles)
    | Ready tiles -> Ready(List.sortWith Tile.compare tiles)
```

**学習ポイント**:
- **OR パターン**: `| Waiting tiles | Ready tiles -> tiles`で重複排除
- **個別処理**: 各状態で適切な型を維持
- **F#イディオム**: 判別共用体との自然な組み合わせ

### 4. エラーハンドリングの簡素化

```fsharp
// Before: 複雑なエラー型
type HandError =
    | InvalidTileCount of actual: int * expected: string
    | TileNotFound of Tile
    | AlreadyDrawn      // 型システムで防止可能
    | NothingToDiscard  // 型システムで防止可能

// After: 型安全により簡素化
type HandError =
    | InvalidTileCount of actual: int * expected: string
    | TileNotFound of Tile
```

**学習ポイント**:
- **型システムによるエラー防止**: 不正な操作自体が不可能
- **Railway-Oriented Programming**: 型安全により失敗パスを削減
- **ドメインエラーの明確化**: 実際に発生しうるエラーのみ表現

## 実際の使用例

### リファクタリング前後の比較

```fsharp
// Before: ランタイムチェック
let result = 
    hand 
    |> tryDraw tile1
    |> Result.bind (tryDiscard tile2)

// After: コンパイル時安全
let result =
    hand           // Waiting hand
    |> draw tile1  // Ready hand
    |> discard tile2  // Result<Waiting hand, HandError>
```

### テストの改善

```fsharp
// Before: 状態チェックが複雑
if count hand = 13 then (* waiting状態 *) else (* ready状態 *)

// After: 型でパターンマッチ
match createWaitingHand () with
| Ok (Waiting _ as waitingHand) -> (* 型安全な処理 *)
| Ok (Ready _) -> failwith "Expected Waiting hand"
```

## 関数型DDD概念の学習成果

### 1. Make Illegal States Unrepresentable（不正状態の表現不可能化）

- **達成**: 13牌と14牌の状態を型レベルで区別
- **具体例**: 13牌の手牌で`discard`を呼ぶことは型エラー
- **利益**: バグの早期発見、ランタイムエラーの劇的削減

### 2. 判別共用体の実践的活用

- **達成**: ドメインの自然な状態を型で表現
- **具体例**: `Waiting | Ready`でツモ前後の状態を区別
- **利益**: ビジネスロジックとコードの完全な一致

### 3. パターンマッチングによるコード重複の解決

- **達成**: 共通操作とタイプ固有操作の適切な分離
- **具体例**: `getTiles`での OR パターン活用
- **利益**: DRY原則の遵守と型安全性の両立

### 4. 型駆動リファクタリング

- **達成**: 既存実装を型安全な設計に進化
- **具体例**: 単一`Hand`型から判別共用体への移行
- **利益**: 段階的改善による安全なコード進化

## 設計判断の学び

### なぜ判別共用体を選んだか

**他の選択肢との比較:**
1. **プライベート型+ラッパー関数**: 冗長、重複が多い
2. **インターフェース**: F#らしくない、複雑
3. **判別共用体**: F#の強み、シンプル、型安全

**判別共用体の利点:**
- F#の言語特性に最適化
- パターンマッチングとの自然な組み合わせ
- 状態遷移の明確な表現

### 実装での具体的工夫

```fsharp
// 状態判定関数（型安全）
let isWaiting = function
    | Waiting _ -> true
    | Ready _ -> false

// 外部ライブラリとの互換性のため count 関数は維持
let count hand = hand |> getTiles |> List.length
```

## 次のステップ

フェーズ1.3では以下を実装予定：
- 包括的なバリデーションテスト
- エッジケースの検証
- パフォーマンステスト

フェーズ2では：
- Meld（面子）検出での判別共用体活用
- より複雑な状態管理の実践

## ファイル構成

```text
src/FunctionalDddMahjong.Domain/
├── Tile.fs                    # フェーズ1.1: 基本型定義
└── Hand.fs                    # フェーズ1.2: 手牌管理（判別共用体）

tests/FunctionalDddMahjong.Domain.Tests/
├── Tests.fs                   # 基本テスト
└── HandTests.fs              # Hand専用テスト（型安全）
```

## 重要な学習ポイント

1. **型システムの力**: コンパイラが不正状態を防ぐ設計
2. **F#イディオム**: 判別共用体+パターンマッチングの威力
3. **ドメイン表現**: ビジネスルールが型定義に直接反映
4. **段階的改善**: 既存コードを型安全に進化させる手法

このフェーズを通じて、「型で設計し、コンパイラに守らせる」関数型DDDの核心を実践的に習得しました。