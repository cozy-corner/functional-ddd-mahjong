# 麻雀で学ぶ関数型ドメインモデリング - 学習ガイド

## 概要

このガイドでは、麻雀を題材にScott Wlaschinの「Domain Modeling Made Functional」のアプローチを学習します。F#を使用して、関数型DDDの核心概念を段階的に習得していきます。

## 学習目標

- 型駆動設計による不正状態の排除
- Railway-Oriented Programming によるエラーハンドリング
- 関数合成による複雑なドメインロジックの構築
- 純粋関数による副作用の制御

## 前提条件

### 技術スタック
- **言語**: F# 8
- **フレームワーク**: .NET 8
- **開発環境**: Visual Studio Code + Ionide
- **テスト**: xUnit

### 用語・命名規則
- [Japanese Mahjong Wikipedia](https://en.wikipedia.org/wiki/Japanese_mahjong) に準拠
- 型名・関数名は英語で統一
- コメントは日本語も可

### 除外するルール
学習目的のため、以下の複雑なルールは扱いません：
- **変形役**: 七対子、国士無双（基本の4面子1雀頭パターンのみ扱う）
- **親子の概念**: プレイヤー間の関係性や席順（単独手牌の役判定のみ）
- **ゲーム状態依存の役**: リーチ、一発など（手牌構成のみで判定可能な役に限定）
- **符計算**: 40符固定とし、翻数の合計のみ計算
- 流し満貫、人和などの特殊役
- 場の状況（風、ドラ表示牌等）
- 鳴き（最初は13牌手牌のみ）

## 学習の進行順序

詳細な学習進行については、[学習進行TODO](./todo.md) を参照してください。

### 概要
1. **Tile型定義 → Hand管理** - 型安全性とドメイン表現
2. **Meld検出 → Winning hand判定** - 関数合成とパターンマッチング  
3. **基本役判定 → 複合役** - Railway-Oriented Programming
4. **点数計算 → 特殊役** - ワークフローとドメインイベント

## Claude Code用プロンプトテンプレート

### 初回プロンプト
```
麻雀の関数型ドメインモデリングをF#で学習したいと思います。Scott Wlaschinの「Domain Modeling Made Functional」のアプローチに従って、段階的に実装していきましょう。

## 現在のフェーズ：[フェーズ番号と名前]

### 学習目標
[該当フェーズの学習目標をここに記載]

### 実装要件
- 用語は https://en.wikipedia.org/wiki/Japanese_mahjong に準拠
- 型安全性を最優先
- 不正な状態を表現不可能にする
- エラーメッセージは日本語で分かりやすく
- 段階的に拡張できる設計
- Result型を使用したエラーハンドリング

### 今回実装する内容
[該当フェーズの実装内容を具体的に記載]

F#プロジェクトのセットアップから始めて、実際に動くコードとテストを書いてください。
```

## 参考資料

- [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) - Scott Wlaschin
- [Japanese Mahjong Wikipedia](https://en.wikipedia.org/wiki/Japanese_mahjong)
- [F# for Fun and Profit](https://fsharpforfunandprofit.com/) - Scott Wlaschin's Blog
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)

## 次のステップ

各フェーズ完了後：
1. 実装したコードのレビュー
2. 関数型DDD概念の理解確認
3. 次フェーズへの準備
4. 必要に応じてリファクタリング

このガイドを通じて、関数型プログラミングとドメイン駆動設計の強力な組み合わせを実践的に学習していきましょう。
