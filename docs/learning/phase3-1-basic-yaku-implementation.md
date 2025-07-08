# フェーズ3.1: 基本役判定の実装 - 学習記録

## 実装内容
タンヤオ（断么九）役の判定機能をRailway-Oriented Programmingで実装

## 主要な学習成果

### 1. 型安全性による設計改善
**問題**: 最初の実装では、役判定に不適切な状態（13牌、非和了手）でもエラーハンドリングが必要だった

```fsharp
// 問題のある初期設計
let checkTanyao hand =
    match hand with
    | Waiting _ -> Error(InvalidHandState ...)  // 毎回チェック
    | Ready tiles ->
        match Hand.tryDecomposeAll hand with
        | [] -> Error NoValidDecomposition      // 毎回チェック
        | _ -> // 役判定
```

**解決**: WinningHand型を導入して「不正な状態を表現不可能」に

```fsharp
// 改善された設計
type WinningHand = private WinningHand of (Meld.Meld list * Pair.Pair) list

let checkTanyao (winningHand: Hand.WinningHand) : Option<Yaku> =
    // エラーハンドリング不要、純粋な役判定
```

### 2. エラーと正常な結果の適切な区別
**学習**: 「役が成立しない」ことは「エラー」ではなく正常な結果

```fsharp
// 不適切: 役不成立をエラーとして扱う
| Error(NotAllSimples terminalOrHonorTiles)  // これは正常な結果

// 適切: Option型で成立/不成立を表現
if allTiles |> List.forall isSimple then Some Tanyao else None
```

### 3. 関数型DDD原則の実践
- **型による制約**: WinningHand型で和了形のみを受け入れ
- **責務の分離**: 和了判定と役判定を分離
- **不正状態の排除**: 型レベルで実行時エラーを防止

### 4. アルゴリズムの最適化
**気づき**: タンヤオは面子の組み合わせ方に無関係

```fsharp
// 不要な複雑さ
decompositions
|> List.exists (fun decomposition -> ...)  // 全分解パターンをチェック

// シンプルな実装
let (melds, pair) = List.head decompositions  // 任意の分解パターン
let allTiles = getAllTilesFromDecomposition (melds, pair)
if allTiles |> List.forall isSimple then Some Tanyao else None
```

**理由**: どの分解パターンでも使用する14牌は同じため

## 技術的な実装詳細

### WinningHand型の設計
```fsharp
// プライベートコンストラクタで型安全性を確保
type WinningHand = private WinningHand of (Meld.Meld list * Pair.Pair) list

// 和了形の作成（失敗する可能性）
let tryCreateWinningHand hand =
    match tryDecomposeAll hand with
    | [] -> None
    | decompositions -> Some(WinningHand decompositions)
```

### タンヤオ判定の実装
```fsharp
let checkTanyao (winningHand: Hand.WinningHand) : Option<Yaku> =
    let decompositions = Hand.getDecompositions winningHand
    let (melds, pair) = List.head decompositions
    let allTiles = getAllTilesFromDecomposition (melds, pair)
    
    if allTiles |> List.forall isSimple then Some Tanyao else None

let private isSimple tile =
    match Tile.getValue tile with
    | Character value | Circle value | Bamboo value ->
        match value with
        | Two | Three | Four | Five | Six | Seven | Eight -> true
        | _ -> false
    | Honor _ -> false
```

## 設計原則の確立

### 1. Railway-Oriented Programming
- **Result型**: 真のエラー（型制約違反）のみに使用
- **Option型**: 正常な成功/失敗の表現に使用

### 2. 型駆動設計
- **Value Object**: WinningHand型で有効な状態のみを表現
- **スマートコンストラクタ**: tryCreateWinningHandで安全な作成

### 3. 関数合成
- **小さな関数**: isSimple, getAllTilesFromDecomposition
- **合成**: checkTanyaoで組み合わせ

## 次のステップへの準備
- **拡張性**: 他の役（ピンフ、トイトイ、ホンイツ）も同じパターンで実装可能
- **共通化**: getAllTilesFromDecomposition等のヘルパー関数を再利用
- **テストパターン**: 既存パターンとの一貫性を保った設計

## 重要な学び
1. **型安全性がエラーハンドリングを簡素化**: 不正な状態を型で防ぐ
2. **ドメインロジックの理解が実装を簡素化**: タンヤオの性質を理解すれば最適化可能
3. **関数型DDD原則の実践的価値**: 理論だけでなく実装品質の向上に直結

この実装により、型安全でシンプル、かつ拡張可能な役判定システムの基盤が完成した。