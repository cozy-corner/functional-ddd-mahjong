# フェーズ2.2: 基本的な面子分解の学習記録

## 実装日: 2025-01-02

## 学習目標
DDDの責務分離と、F#の関数合成・Option型を活用した面子分解の基本実装を学ぶ。

## 主要な学習ポイント

### 1. DDDにおける責務の適切な配置

#### 設計判断：どこに実装すべきか
```fsharp
// ❌ 最初の考え：独立したMeldDecompositionモジュール
module MeldDecomposition =
    let tryDecompose (hand: Hand) : Result<Decomposition, Error> = ...

// ✅ 改善後：Handの責務として配置
module Hand =
    let tryDecompose hand = ...  // ドメインの自然な操作
```

#### 学んだこと
- 面子分解は「手牌の知識」であり、`Hand`モジュールの責務
- 複雑なアルゴリズムは内部実装として分離
- DDDでは「データと振る舞いを一緒に配置」が原則

### 2. Option型による存在の表現

#### Option型の適切な使用
```fsharp
// 分解結果：存在するかもしれない
let tryDecompose hand : (Meld list * Pair) option = ...

// 刻子検索：見つからないかもしれない
let tryFindTriplet tiles : (Meld * Tile list) option = ...
```

#### Result型との使い分け
- `Option`: 単純な存在/非存在（理由は不要）
- `Result`: エラーの詳細が必要な場合

#### 学んだこと
- Option型はnullの安全な代替
- 型システムで「値がないケース」を強制的に処理させる
- F#独特の`Some`/`None`構文

### 3. 関数型プログラミングの実践

#### パイプライン演算子による読みやすいコード
```fsharp
let findPairCandidates tiles =
    tiles
    |> List.groupBy id                    // グループ化
    |> List.filter (fun (_, group) -> List.length group >= 2)  // フィルタ
    |> List.map fst                       // 抽出
```

#### 高階関数の活用
```fsharp
// List.tryPick: 最初のSomeを見つけて返す
|> List.tryPick (fun pairTile ->
    match Pair.tryCreatePair [pairTile; pairTile] with
    | Ok pair -> Some (melds, pair)
    | Error _ -> None)
```

#### 学んだこと
- データ変換パイプライン = 関数合成の実用形
- 高階関数（`tryPick`, `groupBy`, `filter`など）の威力
- 左から右へ読める処理の流れ

### 4. F#特有の構文とイディオム

#### `function`キーワード
```fsharp
// パターンマッチング専用のラムダ式
let rec remove n acc =
    function
    | [] -> List.rev acc
    | h :: t -> if h = item && n > 0 then ...
```

#### タプルによる複数戻り値
```fsharp
// Kotlinの Pair に近い概念
let result: (Meld * Tile list) option = ...
let (meld, remaining) = result  // 分解代入
```

#### 学んだこと
- `function`はF#独特のパターンマッチング記法
- タプルは軽量な複数値の組み合わせ手段
- パターンマッチングによる分解代入

### 5. テスト設計における注意点

#### アサーションの具体性
```fsharp
// ❌ 不十分
| Error(InvalidTileCount(12, _)) -> ()

// ✅ 改善
| Error(InvalidTileCount(actual, expected)) -> 
    Assert.Equal(12, actual)
    Assert.Equal("13 (initial deal)", expected)
```

#### テストでのOR条件の問題
- アサーションでのOR演算子は筋が悪い
- テストケースを一意に決まるものに変更すべき
- 期待値が不確定なテストは保守性が悪い

#### 学んだこと
- テストは明確で予測可能であるべき
- エラーメッセージを具体的に検証
- デバッグしやすいアサーションの重要性

### 6. 内部実装の分離戦略

#### internalモジュールの活用
```fsharp
module internal MeldDecomposition =
    let tryDecomposeInternal tiles = ...
```

#### 公開APIとの関係
```fsharp
// 公開API
module Hand =
    let tryDecompose hand =
        // 内部実装に委譲
        MeldDecomposition.tryDecomposeInternal tiles
```

#### 学んだこと
- 複雑なアルゴリズムは内部実装として隠蔽
- 公開APIはシンプルに保つ
- `InternalsVisibleTo`という選択肢もある

## 技術的な発見

### Option型とパターンマッチングの威力
- nullチェックが不要になる
- コンパイラが全ケース処理を強制
- 型安全性の向上

### 関数合成によるコードの簡潔性
- 複雑な処理を小さな関数の組み合わせで表現
- 再利用可能なヘルパー関数
- テストしやすい粒度の分割

### F#の表現力
- `function`キーワードによる簡潔なパターンマッチング
- パイプ演算子による読みやすいデータ変換
- タプルによる自然な複数戻り値

## 今後の課題

1. **アルゴリズムの改善**
   - 現在は貪欲法（最初に見つかった解を採用）
   - バックトラッキングによる全探索が必要
   - 複数の分解パターンがある場合の対応

2. **パフォーマンスの考慮**
   - 大量の牌に対する効率性
   - メモ化などの最適化手法
   - 末尾再帰の活用

3. **テストカバレッジの拡充**
   - エッジケースの追加
   - パフォーマンステスト
   - プロパティベーステスト

## まとめ

フェーズ2.2では、DDDの責務分離とF#の関数型プログラミングの実践を通じて、保守しやすく型安全なコードを実装できた。特に、Option型による安全な値の表現と、関数合成による読みやすいデータ変換パイプラインの構築は、F#の強力な特徴を実感できる内容だった。

次のフェーズでは、より複雑なアルゴリズム（バックトラッキング）と、Railway-Oriented Programmingによるエラーハンドリングの向上に取り組む予定。