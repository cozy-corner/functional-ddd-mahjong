# Phase 2.2: バックトラッキング実装（PR2の一部）

## 実装目標
MeldDecompositionモジュールの貪欲法をバックトラッキングに改善

## 問題の発見
既存の貪欲法では、最初の選択が悪いと正しい分解ができないケースが存在：
- `22234567m`: 222を刻子にすると34567mが残って失敗
- 正解: 22を雀頭、234+567の順子にする

## 学習内容

### 1. バックトラッキングアルゴリズムの理解
```fsharp
let rec backtrack remaining foundMelds =
    match List.length foundMelds, List.length remaining with
    | 4, 0 -> Some foundMelds // 完成
    | 4, _ -> None // 余り牌がある
    | _, n when n < 3 -> None // 残り牌が足りない
    | _ -> // 全ての候補を試す
```

**キーポイント**:
- 候補を順番に試し、失敗したら次を試す
- 成功したら即座に結果を返す（短絡評価）

### 2. F#関数型プログラミングの実践

#### if-else から match式への改善
**理由**: パターンマッチングの方がF#らしく、可読性が高い

#### インデックス除去
**Before**: `List.mapi`でインデックスを使用
**After**: 再帰的リスト処理
**学び**: 状態に依存しない純粋関数的アプローチ

#### 関数配置の改善
宣言と使用箇所を近づけることで可読性向上

### 3. 重要な概念の理解

#### `tryPick` vs `choose`
- **tryPick**: 最初のSomeで停止
- **choose**: 全てのSomeを収集
- バックトラッキングでは全候補が必要なので`choose`

#### アキュムレータパターン
```fsharp
// acc = accumulator（蓄積変数）
findSequences tailForT3 (sequencesWithT2 @ acc)
```

#### リスト連結（`@`）
2つのリストを連結する演算子

### 4. テストケース設計

#### パラメータ化テストの活用
個別テスト3つ → 1つのTheoryテストに統合
```fsharp
[<Theory>]
[<InlineData("2m,2m,2m,3m,4m,5m,6m,7m,1p,1p,1p,E,E,E", "234,567", "2萬")>]
[<InlineData("1m,1m,2m,2m,3m,3m,5p,6p,7p,1s,1s,1s,N,N", "123,123", "北")>]
[<InlineData("2m,3m,4m,4m,5m,6m,1p,2p,3p,7s,8s,9s,E,E", "234,456", "東")>]
```

#### テストケースの選定
1. **22234567m**: 基本的なバックトラッキング必須
2. **112233m**: 一盃口パターン
3. **234456m**: 重なる順子パターン

### 5. 核心アルゴリズムの理解
```fsharp
let rec tryMelds = function
    | [] -> None  // 候補なし = 失敗
    | (meld, newRemaining) :: rest ->
        match backtrack newRemaining (meld :: foundMelds) with
        | Some result -> Some result  // 成功で即終了
        | None -> tryMelds rest      // 失敗で次を試す
```

**重要**: `newRemaining`は選択した面子3枚を除いた残り牌

### 6. 計算量の考慮
- 理論上O(3^n)だが、麻雀の制約で実用的
- 早期終了により効率化

## 技術的成果
- 貪欲法 → バックトラッキングへの改善
- より関数型らしいF#コード
- パラメータ化テストによる効率化
- 124個全テスト合格

## 残課題（Phase 2.2完了に向けて）
- 複数分解パターンの対応
- パフォーマンステスト
- エッジケースの追加

## まとめ
今回の実装により、貪欲法では解けない麻雀パターンを正しく分解できるようになった。バックトラッキングアルゴリズムの理解と、F#関数型プログラミングの知見を深めることができた。