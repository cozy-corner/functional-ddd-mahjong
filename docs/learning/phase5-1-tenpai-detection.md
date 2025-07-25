# Phase 5.1: テンパイ検出実装の学習記録

## 実装概要

Phase 5.1では、13牌からテンパイ（聴牌）状態を検出し、待ち牌を取得する機能を実装しました。パターンベースの賢いアプローチを採用し、総当たりではなく麻雀の理論に基づいた効率的な検出を実現しました。

## 主要な実装内容

### 1. TenpaiAnalyzer モジュールの作成

#### 型定義
```fsharp
type TenpaiPattern =
    | FourMeldsWait of Meld list * Tile // 4面子完成の単騎待ち
    | ThreeMeldsOnePairWait of Meld list * Pair * IncompletePattern

and IncompletePattern =
    | Ryanmen of Tile * Tile * WaitingTiles // 両面待ち
    | Kanchan of Tile * Tile * Tile // 嵌張待ち
    | Penchan of Tile * Tile * Tile // 辺張待ち
    | Shanpon of Tile * Tile // 双碰待ち

and WaitingTiles = Tile list
```

#### 核心的な関数
- `isTenpai: Tile list -> bool` - 13牌からテンパイ判定
- `getWaitingTiles: Tile list -> Tile list` - 待ち牌リストの取得
- `analyzeIncompletePair: Tile list -> IncompletePattern option` - 不完全形の分析

### 2. パターンベースの検出アルゴリズム

従来の総当たり（34種類全てのタイルを試す）ではなく、麻雀の理論に基づいた5つの基本パターンを実装：

1. **両面待ち（Ryanmen）**: 23 → 14待ち
2. **嵌張待ち（Kanchan）**: 13 → 2待ち
3. **辺張待ち（Penchan）**: 12 → 3待ち、89 → 7待ち
4. **双碰待ち（Shanpon）**: 11,22 → 12待ち
5. **単騎待ち（Tanki）**: 4面子完成 + 1枚 → その牌待ち

### 3. MeldDecomposition の最適化

#### 重複問題の解決
初期実装では5184通りの重複パターンが生成されていましたが、関数型プログラミングの `combinations` 関数により重複なし生成を実現：

```fsharp
let rec private combinations n list =
    match n, list with
    | 0, _ -> [[]]
    | _, [] -> []
    | n, x::xs ->
        let withX = combinations (n-1) xs |> List.map (fun combo -> x::combo)
        let withoutX = combinations n xs
        withX @ withoutX
```

結果：5184パターン → 数十パターンに削減、テスト実行時間が8秒 → 1秒に改善

#### バックトラッキングの改良
既存の面子分解アルゴリズムで発見されたバグを修正：
- `findAllSequenceCandidates` が最初の牌からしか順子を探していない問題
- インデックス付き組み合わせによる正確な面子抽出の実装

## 技術的な学習成果

### 1. 関数型アルゴリズムの実践

#### 再帰的組み合わせ生成
重複を除去するのではなく、そもそも重複が生まれないアルゴリズムの実装を学習。エレガントな関数型プログラミングの実践例となりました。

#### パターンマッチングの活用
F#の `match` 式を活用した型安全なパターン分析：

```fsharp
let analyzeIncompletePair tiles =
    match List.sortWith Tile.compare tiles with
    | [t1; t2] when getNextTile t1 = Some t2 ->
        // 連続する2牌 → 両面待ち
        let wait1 = getPrevTile t1
        let wait2 = getNextTile t2
        // ...
```

#### 高階関数による関数型リファクタリング
PRレビューフィードバックを受けて、ミュータブルな`ResizeArray`をイミュータブルなリスト操作に置換：

**`List.choose`**: フィルタリングとマッピングを同時実行
```fsharp
// Before: 命令型スタイル
let patterns = ResizeArray<TenpaiPattern>()
for (melds, remaining) in fourMeldPatterns do
    match remaining with
    | [ single ] -> patterns.Add(FourMeldsWait(melds, single))
    | _ -> ()

// After: 関数型スタイル
let fourMeldsPatterns =
    MeldDecomposition.tryFindNMelds 4 tiles
    |> List.choose (fun (melds, remaining) ->
        match remaining with
        | [ single ] -> Some(FourMeldsWait(melds, single))
        | _ -> None)
```

**`List.collect`**: 各要素から複数結果を生成し連結（flatMap）
```fsharp
// 各雀頭候補から複数のテンパイパターンを生成し、全て連結
let threeMeldsOnePairPatterns =
    findPairCandidates tiles
    |> List.collect (fun pairTile ->
        match Pair.tryCreatePair [ pairTile; pairTile ] with
        | Ok pair ->
            // 内部でList.chooseを使用してパターンリストを生成
        | Error _ -> [])  // エラー時は空リスト
```

**学習ポイント**:
- `List.choose`: `('T -> 'U option) -> 'T list -> 'U list` - 条件付き変換
- `List.collect`: `('T -> 'U list) -> 'T list -> 'U list` - flatMap操作
- 副作用のない純粋関数による実装
- パイプライン演算子による関数合成の美しさ

### 2. 複雑度対応とテスト駆動開発

#### 段階的な複雑度向上
1. **基本パターン**: 単騎、両面、嵌張、辺張、双碰
2. **複合パターン**: 1112形、1113形（複数の解釈が可能）
3. **究極パターン**: 純正九蓮宝燈（9種全待ち）

#### テストケースの設計原則
- **シンプル化**: 関係のない牌は字牌に統一
- **期待値検証**: 実装に合わせるのではなく、麻雀理論から正しい期待値を導出
- **エッジケース**: 同じ牌4枚、連続する牌、字牌4面子など

### 3. 性能最適化

#### Before/After比較
- **Before**: 全組み合わせ生成 → 重複除去（5000+パターン、8秒）
- **After**: 重複なし生成（数十パターン、1秒）

#### 最適化手法
- インデックス順序固定による組み合わせ制御
- 早期終了条件の導入
- 不要な計算の排除

## デバッグ手法とエラー対処

### 1. 複雑なバグの単純化
複合パターンで失敗した場合、まず最もシンプルなケースに分解して問題を特定：
```fsharp
// 複雑: E,E,E,S,S,S,W,W,W,1m,1m,1m,2m,3m (失敗)
// ↓
// シンプル: 1m,1m,1m,2p,2p,2p,3s,3s,3s,4s,4s,4s,E (成功)
```

### 2. 期待値の検証
実装結果に合わせるのではなく、麻雀の理論から正しい期待値を導出：
- 1112345形 → 2,3,5,6待ち（実装で1,4を追加してはならない）
- 純正九蓮宝燈 → 1,2,3,4,5,6,7,8,9待ち（全種類）

### 3. 責務の分離
- **MeldDecomposition**: 面子分解のみ（テンパイ概念を含まない）
- **TenpaiAnalyzer**: テンパイ分析（MeldDecompositionを利用）

## 麻雀ドメイン知識の整理

### 1. スコープの明確化
- **対象**: 4面子1雀頭の基本形のみ
- **対象外**: 七対子、国士無双（特殊形）

### 2. テンパイの定義
- **13牌**: テンパイ状態（あと1牌で和了）
- **14牌**: 和了状態（面子分解完了）

### 3. 複合パターンの理解
同じ牌配から複数の解釈が可能な場合があり、全ての有効な待ち牌を検出する必要がある：
- 1112形: 刻子+単騎待ち、雀頭+辺張待ち
- 1113形: 刻子+単騎待ち、雀頭+嵌張待ち

## テストケースの充実度

### 280個のテストケース
1. **基本パターン**: 各待ちパターンの標準的なケース
2. **複合パターン**: 複数解釈が可能なケース
3. **エッジケース**: 同一牌多用、連続牌、字牌集中
4. **究極ケース**: 純正九蓮宝燈（最高難度）
5. **エラーケース**: 牌数不正、ノーテン手

### テスト設計の工夫
- パラメータ化テスト（Theory/InlineData）による効率化
- 字牌統一による可読性向上
- 期待値の明確な根拠

## 次フェーズへの準備

Phase 5.1の完成により、以下の基盤が整いました：
1. **テンパイ検出**: リーチ宣言の前提条件
2. **パターン分析**: 複雑な麻雀ロジックへの対応力
3. **性能最適化**: 実用的な処理速度
4. **型安全性**: コンパイル時エラー検出

これらの基盤を活用して、Phase 5.2（リーチドメインの実装）でRailway-Oriented Programmingの実践に進みます。

## まとめ

Phase 5.1では、関数型ドメイン駆動設計の集大成として、複雑な麻雀のテンパイ検出を実装しました。技術的には再帰、バックトラッキング、パターンマッチング、性能最適化を習得し、ドメイン知識としては麻雀の理論的な理解を深めました。280のテストケースが全て成功していることから、実装の正確性と堅牢性が確認されています。