# フェーズ3.1: 基本役判定の実装計画

## 概要
このフェーズでは、Railway-Oriented Programmingを用いて基本的な麻雀役の判定機能を実装します。

## 学習目標
- Railway-Oriented Programmingの実践
- Result型によるエラーハンドリング
- 関数合成による複雑なロジックの構築
- 型安全な役判定の実装

## 実装する役
1. **タンヤオ（断么九）** - 中張牌（2-8の数牌）のみで構成
2. **ピンフ（平和）** - 順子4つ+雀頭で構成
3. **トイトイ（対々和）** - 刻子4つ+雀頭で構成
4. **ホンイツ（混一色）** - 一色の数牌+字牌で構成

## アーキテクチャ設計

### モジュール構成
1. **Yaku.fs** - 役の型定義と基本判定関数
   - `Yaku`型（判別共用体）で役を表現
   - `YakuError`型でエラーを表現
   - 各役の個別判定関数（Result型を返す）

2. **YakuAnalyzer.fs** - 役の分析と合成
   - 手牌から全ての成立役を検出
   - 複数の役判定を合成
   - Railway-Oriented Programmingパターンの実装

### 型定義（予定）
```fsharp
// 役の型定義
type Yaku =
    | Tanyao    // 断么九
    | Pinfu     // 平和
    | Toitoi    // 対々和
    | Honitsu   // 混一色

// 役判定のエラー型
type YakuError =
    | InvalidHandState of string
    | NoValidDecomposition
    | NotAllSimples of Tile list
    | NotAllSequences of Meld list
    | NotAllTriplets of Meld list
    | NotOneSuitWithHonors
```

## 実装順序（PR単位）

### PR1: タンヤオ実装
- Yaku.fs の作成（型定義、エラー型）
- タンヤオ判定の実装（最もシンプルな役）
- タンヤオのテスト実装

### PR2: 面子構成系の役
- ピンフ判定（順子4つ+雀頭）
- トイトイ判定（刻子4つ）
- 各役のテスト実装

### PR3: 牌の種類系の役
- ホンイツ判定（一色+字牌）
- チンイツ判定（一色のみ）
- 各役のテスト実装

### PR4: YakuAnalyzer実装
- YakuAnalyzer.fs の作成
- 複数役の並列判定
- Railway-Oriented Programmingの実践
- 統合テスト（複数役の組み合わせ）

## Railway-Oriented Programming の適用

### 基本パターン
```fsharp
// Result型のチェーン
let checkYaku hand =
    hand
    |> validateHand
    |> Result.bind decomposeHand
    |> Result.bind checkTanyao
    |> Result.map (fun _ -> Tanyao)
```

### 複数役の合成
```fsharp
// 並列的な役判定
let analyzeAllYaku hand =
    [
        checkTanyao hand
        checkPinfu hand
        checkToitoi hand
        checkHonitsu hand
    ]
    |> List.choose (function Ok yaku -> Some yaku | Error _ -> None)
```

## 技術的なポイント

### 1. 純粋関数の設計
- 各判定関数は副作用なし
- 入力（手牌）から出力（Result<Yaku, YakuError>）への明確な変換

### 2. エラーの明示的な表現
- 役が成立しない理由を型で表現
- デバッグとテストが容易

### 3. 合成可能性
- 小さな判定関数を組み合わせて複雑な判定を構築
- 新しい役の追加が容易

### 4. 型安全性
- 不正な状態を型レベルで防ぐ
- コンパイル時に多くのバグを検出

## 次のステップ
フェーズ3.2では、より複雑な役（チンイツ、サンアンコ）や複合役の処理を実装し、Railway-Oriented Programmingをさらに深く学習します。