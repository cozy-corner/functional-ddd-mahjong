# 関数型アーキテクチャ設計ガイド

## 概要

このドキュメントは、「Domain Modeling Made Functional」の推奨する関数型アーキテクチャに基づいた、プロジェクト構造の設計ガイドです。

## 現在の構造の問題点

### 1. 単一ドメインプロジェクトへの過度な集中
現在、全てのコードが `FunctionalDddMahjong.Domain` プロジェクトに集約されており、以下の問題があります：

- 責務の境界が不明確
- 依存関係が複雑化
- テストの粒度が大きくなりがち
- 将来の拡張が困難

### 2. 責務の混在
単一プロジェクト内に以下の異なる責務が混在しています：

- **基本型**（Tile, Meld, Pair）
- **ドメインロジック**（YakuAnalyzer）
- **ワークフロー**（Reach）
- **汎用ユーティリティ**（Validation）

## 推奨アーキテクチャ構造

### プロジェクト構成

```
src/
├── FunctionalDddMahjong.SharedKernel/     # 共有カーネル
│   ├── Result.fs                          # Result型拡張
│   └── Validation.fs                      # 汎用バリデーション
│
├── FunctionalDddMahjong.Domain/           # 純粋なドメインモデル
│   ├── Types.fs                           # 基本型定義
│   ├── Tile.fs                            # 牌の型と操作
│   ├── Meld.fs                            # 面子の型と操作
│   ├── Pair.fs                            # 雀頭の型と操作
│   ├── Hand.fs                            # 手牌の型と操作
│   └── Yaku.fs                            # 役の型定義
│
├── FunctionalDddMahjong.DomainServices/   # ドメインサービス
│   ├── MeldDecomposition.fs               # 面子分解アルゴリズム
│   ├── TenpaiAnalyzer.fs                  # テンパイ分析
│   └── YakuAnalyzer.fs                    # 役判定と分析
│
├── FunctionalDddMahjong.Workflows/        # ワークフロー
│   ├── ReachWorkflow.fs                   # リーチ宣言ワークフロー
│   └── ScoringWorkflow.fs                 # 得点計算ワークフロー
│
├── FunctionalDddMahjong.Api/              # APIレイヤー
│   ├── Dto.fs                             # データ転送オブジェクト
│   └── Serialization.fs                   # シリアライゼーション
│
└── FunctionalDddMahjong.Infrastructure/   # インフラストラクチャ
    └── Persistence.fs                     # 永続化
```

### 各レイヤーの責務

#### 1. SharedKernel（共有カーネル）
- **責務**: ドメインに依存しない汎用的なユーティリティ
- **内容**: Result型の拡張、汎用的なバリデーション関数
- **依存**: なし

#### 2. Domain（純粋なドメインモデル）
- **責務**: ビジネスルールを表現する型と基本操作
- **内容**: 値オブジェクト、エンティティ、ドメインプリミティブ
- **依存**: SharedKernelのみ

#### 3. DomainServices（ドメインサービス）
- **責務**: 複数のドメインオブジェクトにまたがる複雑なロジック
- **内容**: アルゴリズム、分析ロジック、複雑な計算
- **依存**: Domain, SharedKernel

#### 4. Workflows（ワークフロー）
- **責務**: ビジネスプロセスの実装
- **内容**: ユースケースの実装、プロセスの調整
- **依存**: DomainServices, Domain, SharedKernel

#### 5. Api（APIレイヤー）
- **責務**: 外部とのインターフェース
- **内容**: DTO定義、シリアライゼーション、デシリアライゼーション
- **依存**: Workflows, Domain

#### 6. Infrastructure（インフラストラクチャ）
- **責務**: 技術的関心事の実装
- **内容**: データベースアクセス、外部サービス連携
- **依存**: Domain（インターフェース経由）

## 依存関係の原則

### 依存の方向
```
Infrastructure → Api → Workflows → DomainServices → Domain → SharedKernel
```

- 依存は常に内側（ドメイン）に向かう
- 外側のレイヤーは内側のレイヤーを知っているが、逆は成立しない
- ドメインは技術的詳細から完全に独立

### F#プロジェクトの依存順序
F#では、ファイルの順序が重要です。各プロジェクト内でも適切な順序を維持：

```fsproj
<ItemGroup>
  <!-- 基本型から順に -->
  <Compile Include="Types.fs" />
  <Compile Include="Tile.fs" />
  <Compile Include="Meld.fs" />
  <Compile Include="Pair.fs" />
  <Compile Include="Hand.fs" />
  <Compile Include="Yaku.fs" />
</ItemGroup>
```

## 移行計画

### フェーズ6.1: SharedKernelの分離
1. `FunctionalDddMahjong.SharedKernel` プロジェクト作成
2. `Validation.fs` を移動
3. Result型の拡張関数を追加

### フェーズ6.2: DomainServicesの分離
1. `FunctionalDddMahjong.DomainServices` プロジェクト作成
2. アルゴリズム系モジュールを移動：
   - MeldDecomposition.fs
   - TenpaiAnalyzer.fs
   - YakuAnalyzer.fs

### フェーズ6.3: Workflowsの分離
1. `FunctionalDddMahjong.Workflows` プロジェクト作成
2. `Reach.fs` を `ReachWorkflow.fs` として移動

### フェーズ6.4: プロジェクト構造の最終確認
1. 依存関係の検証
2. 各レイヤーの責務確認
3. ドキュメント整備

## 利点

### 1. 明確な責務分離
各プロジェクトが単一の責務を持ち、理解しやすくなります。

### 2. 依存関係の明確化
プロジェクト参照により、依存関係が可視化され、循環参照を防げます。

### 3. テスタビリティの向上
各レイヤーを独立してテストでき、モックの作成も容易になります。

### 4. 将来の拡張性
新機能の追加時に、適切なレイヤーが明確で、影響範囲を限定できます。

### 5. チーム開発への対応
複数人での開発時に、レイヤーごとに担当を分けやすくなります。

## 参考資料

- [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) - Chapter 3: Functional Architecture
- [F# for Fun and Profit - Organizing modules in a project](https://fsharpforfunandprofit.com/posts/recipe-part3/)

## todo.mdとの対応

このアーキテクチャ設計は、`todo.md` のフェーズ6で実装予定です。各フェーズの実装内容と、対応するアーキテクチャレイヤーは以下の通りです：

### 実装済みフェーズとレイヤー対応
- **フェーズ1-2**: Domain層の基本型（Tile, Meld, Pair, Hand）
- **フェーズ3**: Domain層の役定義とDomainServices層の役分析
- **フェーズ5**: Workflows層のリーチワークフロー

### 今後の実装予定
- **フェーズ6**: アーキテクチャリファクタリング（本ドキュメントの内容）
- **フェーズ7**: Api層とInfrastructure層の実装