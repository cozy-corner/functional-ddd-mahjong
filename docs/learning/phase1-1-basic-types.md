# フェーズ1.1 基本型定義 - 学習成果

## 概要

麻雀牌を表現する型安全なドメインモデルを実装し、関数型DDDの基礎概念を学習しました。

## 実装内容

### 1. 型駆動設計による不正状態の排除

```fsharp
// 数字牌の値（1-9のみ有効）
type NumberValue = 
    | One | Two | Three | Four | Five 
    | Six | Seven | Eight | Nine

// 字牌の種類
type HonorType =
    | East | South | West | North  // 風牌
    | White | Green | Red          // 三元牌

// 牌の種類
type TileType =
    | Character of NumberValue  // 萬子
    | Circle of NumberValue     // 筒子
    | Bamboo of NumberValue     // 索子
    | Honor of HonorType        // 字牌
```

**学習ポイント**: 不正な牌（萬子の10、筒子の0など）はコンパイル時に作成不可能

### 2. Value Object パターンの実装

```fsharp
// 牌を表現する型（Value Object）
type Tile = private Tile of TileType

// 牌作成関数（スマートコンストラクタ）
let create tileType = Tile tileType

// 牌の内容を取得
let getValue (Tile tileType) = tileType
```

**学習ポイント**: 
- プライベートコンストラクタで内部状態を保護
- スマートコンストラクタで安全な作成
- 不変性の保証

### 3. Railway-Oriented Programming の基礎実装

```fsharp
// エラー型定義
type TileError =
    | InvalidNumberValue of int
    | InvalidHonorName of string
    | InvalidTileString of string

// Result型を使った安全な牌作成関数
let tryCreateFromNumber tileType number =
    match number with
    | 1 -> Ok (create (tileType One))
    | 2 -> Ok (create (tileType Two))
    // ... 3-9
    | n -> Error (InvalidNumberValue n)

// 文字列から牌を作成（例：'1m', '2p', '3s', 'E'）
let tryParseFromString (str: string) =
    match str.ToUpper() with
    | s when s.EndsWith("M") && s.Length = 2 ->
        // 萬子の処理
    | "E" -> Ok (create (Honor East))
    | _ -> Error (InvalidTileString str)
```

**学習ポイント**:
- `Result<Tile, TileError>`による明示的エラーハンドリング
- 成功パス（Ok）と失敗パス（Error）の分離
- 例外に頼らない関数型エラー処理

## 実際の使用例

```fsharp
// 正常な牌作成
let tile1 = create (Character Five)  // 五萬
let tile2 = create (Honor East)      // 東

// 安全な牌作成（Result型）
let result1 = tryCreateFromNumber Character 5  // Ok(五萬)
let result2 = tryCreateFromNumber Character 10 // Error(InvalidNumberValue 10)

// 文字列からの作成
let result3 = tryParseFromString "5m"  // Ok(五萬)
let result4 = tryParseFromString "10m" // Error(InvalidTileString "10m")
let result5 = tryParseFromString "E"   // Ok(東)
```

## 関数型DDD概念の学習成果

### 1. 型駆動設計（Type-Driven Design）
- **達成**: 不正な状態をコンパイル時に防ぐ設計
- **具体例**: 萬子の10や筒子の0は型システムレベルで作成不可
- **利益**: バグの早期発見、ランタイムエラーの削減

### 2. Value Object パターン
- **達成**: 不変で同一性を持たない値オブジェクトの実装
- **具体例**: `Tile`型のプライベートコンストラクタと不変性
- **利益**: 副作用のない安全な値の操作

### 3. Railway-Oriented Programming の基礎
- **達成**: Result型による明示的エラーハンドリング
- **具体例**: `Result<Tile, TileError>`によるエラーの型安全な表現
- **利益**: エラー処理の明確化、例外への依存削減

### 4. ドメイン表現の忠実性
- **達成**: 麻雀の実際のルールが型定義に直接反映
- **具体例**: Character/Circle/Bamboo/Honor の明確な分類
- **利益**: ビジネスロジックとコードの一致、保守性向上

## 次のステップ

フェーズ1.2では以下を実装予定：
- Hand型の定義（14牌の制約）
- 手牌の基本操作（追加、削除、ソート）
- 包括的なテストスイート

## ファイル構成

```
src/FunctionalDddMahjong.Domain/
└── Tile.fs  # 今回実装した基本型定義
```