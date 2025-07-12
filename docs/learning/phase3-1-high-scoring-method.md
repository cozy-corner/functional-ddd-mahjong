# Phase 3.1 - 高点法と相互排他性の実装

## 実装日: 2025-01-12

## 概要
麻雀の高点法（最高得点選択）と相互排他性処理を実装。相互排他関係にある役から最高翻数の役を自動選択する機能を追加。

## 実装内容

### 1. 相互排他関係の定義
```fsharp
let private mutuallyExclusivePairs =
    [
        // 一盃口とトイトイは相互排他（同じ順子2つ vs 全て刻子）
        Set.ofList [ Yaku.Iipeikou; Yaku.Toitoi ]
        // 将来の例: 二盃口とトイトイも相互排他
        // Set.ofList [ Yaku.Ryanpeikou; Yaku.Toitoi ]
    ]
```

### 2. 高点法ロジック
```fsharp
let private resolveExclusivity (allDetectedYaku: Yaku.Yaku list) : Yaku.Yaku list =
    mutuallyExclusivePairs
    |> List.fold (fun currentYaku exclusivePair ->
        // 競合する役を見つけて、最高翻数の役を選択
        match conflictingYaku with
        | [] -> currentYaku  // 競合なし
        | [_] -> currentYaku  // 1つだけなら競合なし
        | multiple ->
            let bestYaku = multiple |> List.maxBy Yaku.getHan
            // 競合する役を除去し、最高翻数の役のみ残す
            currentYaku
            |> List.filter (fun yaku -> not (Set.contains yaku exclusivePair))
            |> fun remaining -> bestYaku :: remaining
    ) allDetectedYaku
```

## 重要な学び

### 1. 相互排他性の本質理解
- **誤解**: 相互排他はエラーや警告
- **正解**: 相互排他は麻雀のルール。どちらも形上は成立しているが、高点法で選択する

**例**: 111222333mRRRGG
- 一盃口解釈: (123m)(123m)(333m)(RR)(GG) - 順子2つ
- トイトイ解釈: (111m)(222m)(333m)(RR)(GG) - 全て刻子
- 両方成立するが、トイトイ（2翻）> 一盃口（1翻）で選択

### 2. アルゴリズム設計の改善

#### 初期実装（非効率）
```fsharp
// 冪集合を生成（2^n通り）
let powerset = ...
allDetectedYaku
|> powerset
|> List.filter hasNoMutualExclusivity
|> List.maxBy totalHan
```
- 計算量: O(2^n)
- 40個の役なら2^40 ≈ 1兆通り

#### 改善後（効率的）
```fsharp
// 相互排他グループごとに直接処理
let resolveExclusivity = ...
```
- 計算量: O(n)
- 実用的で理解しやすい

### 3. 関数命名の重要性
- **問題**: `hasNoMutualExclusivity` - 否定形は分かりにくい
- **改善**: `hasMutualExclusivity` - 肯定形で意図が明確

### 4. テスト設計のベストプラクティス

#### 問題のあったテスト
```fsharp
// パラメータに依存した条件分岐
if tileString.Contains("1m,1m,1m,2m,2m,2m,3m,3m,3m") then
    Assert.DoesNotContain(Yaku.Iipeikou, analysisResult.Yaku)
```

#### 改善後
```fsharp
// 期待値との完全一致、順序無視
Assert.Equal<Set<Yaku.Yaku>>(Set.ofList expectedYakuList, Set.ofList analysisResult.Yaku)
```

### 5. F#特有の技法
- **List.fold**: 状態を蓄積しながらリストを処理
- **パターンマッチング**: `match conflictingYaku with | [] | [_] | multiple`
- **Set比較**: 順序を無視した要素比較

## 実際の動作例

### ケース1: 111222333mRRRGG
1. 検出: [一盃口, トイトイ, ホンイツ]
2. 相互排他チェック: 一盃口 ⇔ トイトイ
3. 高点法選択: トイトイ（2翻）> 一盃口（1翻）
4. 結果: [トイトイ, ホンイツ] = 5翻

### ケース2: 2m,2m,2m,3m,3m,3m,4m,4m,4m,5p,5p,5p,6s,6s
1. 検出: [タンヤオ, 一盃口, トイトイ]
2. 相互排他チェック: 一盃口 ⇔ トイトイ
3. 高点法選択: トイトイ（2翻）> 一盃口（1翻）
4. 結果: [タンヤオ, トイトイ] = 3翻

## 今後の拡張性

### 追加予定の相互排他関係
- 二盃口 ⇔ トイトイ
- 三色同順 ⇔ 三色同刻
- 混一色 ⇔ 清一色（実際は清一色が上位互換）

### 設計の利点
- 新しい相互排他関係はリストに追加するだけ
- O(n)の計算量で効率的
- 麻雀のルールに忠実

## まとめ
相互排他性は「エラー」ではなく「選択」の問題。組み合わせ生成の複雑なアプローチから、直接的で効率的なアプローチへの改善により、理解しやすく拡張しやすい実装を実現した。