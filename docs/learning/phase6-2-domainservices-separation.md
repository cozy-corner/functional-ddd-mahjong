# Phase 6.2: DomainServicesの分離

## 概要

Phase 6.2では、関数型DDDアーキテクチャにおけるDomainServicesとWorkflowsの分離を実装しました。これにより、Clean Architecture/Onion Architectureの関数型版を実現し、適切な依存関係管理と責務分離を達成しました。

## 学習目標

- **関数型アーキテクチャ**: Clean Architecture/Onion Architectureの関数型版実装
- **依存関係管理**: F#プロジェクトにおける適切な参照設定
- **レイヤー分離**: ドメイン、サービス、ワークフローの責務分離
- **モジュール設計**: 高凝集・低結合の実践
- **カプセル化**: internal修飾子による実装詳細の隠蔽

## 実装内容

### アーキテクチャ構造

最終的なアーキテクチャ構造：

```
SharedKernel ← Domain ← DomainServices ← Workflows
```

各レイヤーの責務：

- **SharedKernel**: 汎用的なユーティリティ（Validation.fs等）
- **Domain**: コアドメインロジック（Tile, Hand, Meld, Pair, Yaku等）
- **DomainServices**: 複数ドメインオブジェクトを協調させるサービス（TenpaiAnalyzer, YakuAnalyzer）
- **Workflows**: ドメインサービスを編成するワークフロー（ReachWorkflow）

### 1. DomainServicesプロジェクトの作成

```fsharp
// FunctionalDddMahjong.DomainServices.fsproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="TenpaiAnalyzer.fs" />
    <Compile Include="YakuAnalyzer.fs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\FunctionalDddMahjong.Domain\FunctionalDddMahjong.Domain.fsproj" />
  </ItemGroup>
</Project>
```

### 2. Workflowsプロジェクトの作成

```fsharp
// FunctionalDddMahjong.Workflows.fsproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="ReachWorkflow.fs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\FunctionalDddMahjong.Domain\FunctionalDddMahjong.Domain.fsproj" />
    <ProjectReference Include="..\FunctionalDddMahjong.DomainServices\FunctionalDddMahjong.DomainServices.fsproj" />
    <ProjectReference Include="..\FunctionalDddMahjong.SharedKernel\FunctionalDddMahjong.SharedKernel.fsproj" />
  </ItemGroup>
</Project>
```

### 3. MeldDecompositionのカプセル化

最も重要な設計決定の一つは、`MeldDecomposition.fs`のカプセル化でした：

```fsharp
// MeldDecomposition.fs - internal化
namespace FunctionalDddMahjong.Domain

module internal MeldDecomposition =
    // 複雑なアルゴリズム実装...
    let tryDecomposeAllInternal tiles = ...
    let tryFindNMelds targetCount tiles = ...
```

```fsharp
// Hand.fs - 必要な関数のみ公開
module Hand =
    // 既存のHand機能...
    
    // DomainServicesに必要な関数を公開
    let tryFindNMelds targetCount tiles =
        MeldDecomposition.tryFindNMelds targetCount tiles
    
    let compareTiles = Tile.compare
    
    let tryCreatePairFromTiles tiles = Pair.tryCreatePair tiles
```

**設計判断の理由**：
- **実装隠蔽**: 複雑なアルゴリズム（MeldDecomposition）を内部実装として隠蔽
- **安定インターフェース**: Hand.fsを通じて必要最小限の機能のみ公開
- **依存関係管理**: 外部プロジェクトが低レベルアルゴリズムに直接依存することを防ぐ

## 技術的成果

### 1. 適切な依存関係管理

```fsharp
// TenpaiAnalyzer.fs - DomainServicesレイヤー
namespace FunctionalDddMahjong.DomainServices

module TenpaiAnalyzer =
    open FunctionalDddMahjong.Domain
    open FunctionalDddMahjong.Domain.Tile
    open FunctionalDddMahjong.Domain.Meld
    open FunctionalDddMahjong.Domain.Pair
    
    // Hand.fsを通じて必要な機能を使用
    let isTenpai tiles =
        let patterns = analyzeTenpaiPatterns tiles
        not (List.isEmpty patterns)
    
    let private analyzeTenpaiPatterns tiles =
        // Hand.tryFindNMelds を使用
        Hand.tryFindNMelds 4 tiles
        |> List.choose (fun (melds, remaining) -> ...)
```

### 2. F#モジュールシステムの正しい使用

重要な学習：F#では型とモジュールが同じ名前を持つことができるため、適切な解決が必要：

```fsharp
// ReachWorkflow.fs - 問題の解決
module ReachValidation =
    open FunctionalDddMahjong.Domain
    open FunctionalDddMahjong.Domain.Hand  // 型Handを開く
    open FunctionalDddMahjong.DomainServices
    
    // モジュール Hand を明示的に参照
    module Hand = FunctionalDddMahjong.Domain.Hand
    
    let checkTenpai (hand: Hand.Hand) : Result<unit, ReachError> =
        let tiles = Hand.getTiles hand  // モジュール関数を使用
        if TenpaiAnalyzer.isTenpai tiles then
            Ok()
        else
            Error NotTenpai
```

**完全修飾名の回避**：
- `module Hand = FunctionalDddMahjong.Domain.Hand` によるエイリアス
- `Hand.Hand` 型と `Hand.getTiles` 関数の両方が使用可能

### 3. テストの適切な配置

```
tests/
├── FunctionalDddMahjong.Domain.Tests/           # 233テスト
│   ├── TileTests.fs
│   ├── HandTests.fs
│   ├── MeldTests.fs
│   └── ...
└── FunctionalDddMahjong.DomainServices.Tests/   # 76テスト
    ├── TenpaiAnalyzerTests.fs
    ├── YakuAnalyzerTests.fs
    └── HandDecompositionTests.fs
```

**テスト配置の原則**：
- **Domain.Tests**: 純粋なドメインロジックのテスト
- **DomainServices.Tests**: 複数ドメインオブジェクトの協調をテスト

## 遭遇した課題と解決策

### 1. プロジェクトファイルの管理

**課題**: 新しいプロジェクトがソリューションに登録されていない
**解決**: `dotnet sln add` コマンドによる適切なプロジェクト登録

```bash
dotnet sln add src/FunctionalDddMahjong.DomainServices/FunctionalDddMahjong.DomainServices.fsproj
dotnet sln add src/FunctionalDddMahjong.Workflows/FunctionalDddMahjong.Workflows.fsproj
dotnet sln add tests/FunctionalDddMahjong.DomainServices.Tests/FunctionalDddMahjong.DomainServices.Tests.fsproj
```

### 2. 名前空間とモジュールの混乱

**課題**: `open FunctionalDddMahjong.Domain.Hand` が型のみを開き、モジュール関数が使用できない
**解決**: モジュールエイリアスによる明示的な参照

```fsharp
// 問題のあるコード
open FunctionalDddMahjong.Domain.Hand
let tiles = Hand.getTiles hand  // エラー: Hand module not found

// 解決されたコード
open FunctionalDddMahjong.Domain.Hand
module Hand = FunctionalDddMahjong.Domain.Hand
let tiles = Hand.getTiles hand  // 正常動作
```

### 3. テスト参照の修正

**課題**: 移動したモジュールの参照が正しく更新されていない
**解決**: 適切な名前空間とモジュール参照の修正

```fsharp
// 修正前
open FunctionalDddMahjong.DomainServices.YakuAnalyzer
open FunctionalDddMahjong.DomainServices.MeldDecomposition

// 修正後
open FunctionalDddMahjong.DomainServices
open FunctionalDddMahjong.DomainServices.YakuAnalyzer
```

## 設計原則の実践

### 1. 依存関係逆転の原則

```
Workflows → DomainServices → Domain ← SharedKernel
```

- 各層は内側の層にのみ依存
- 外側の層は内側の層を参照しない
- 安定した依存関係の実現

### 2. 単一責任の原則

- **Domain**: 基本的なドメインオブジェクト
- **DomainServices**: 複数ドメインオブジェクトの協調
- **Workflows**: ビジネスプロセスの編成

### 3. インターフェース分離の原則

```fsharp
// Hand.fs - 必要最小限の公開関数
let tryFindNMelds targetCount tiles = ...
let compareTiles = Tile.compare
let tryCreatePairFromTiles tiles = ...
```

## 実用的な価値

このアーキテクチャパターンは実世界の大規模アプリケーションで使用可能：

1. **保守性**: 各レイヤーが明確な責務を持つ
2. **テスト可能性**: 各層を独立してテスト可能
3. **拡張性**: 新しいドメインサービスやワークフローの追加が容易
4. **理解しやすさ**: 依存関係が明確で新しいメンバーが理解しやすい

## 学習成果

### 技術的スキル

- **F#プロジェクト管理**: 複数プロジェクトの適切な構成
- **名前空間設計**: 論理的で理解しやすい名前空間構造
- **モジュールシステム**: F#の型とモジュールの違いの理解
- **依存関係管理**: 内側に向かう依存の実践

### 設計スキル

- **アーキテクチャ分離**: Clean Architectureの関数型版実装
- **責務分離**: 各層の適切な責務の識別と配置
- **カプセル化**: 実装詳細の隠蔽とインターフェース設計
- **テスト設計**: 責務に応じたテスト配置

## 次のステップ

Phase 6.2の完了により、堅牢で保守性の高いアーキテクチャが確立されました。次の段階では：

1. **永続化レイヤー**: データベースとの連携
2. **APIレイヤー**: RESTful APIの実装
3. **統合テスト**: 層をまたいだテストの実装
4. **性能最適化**: 実際の使用パターンに基づく最適化

このアーキテクチャは、実世界の複雑なアプリケーションの基盤として使用できる品質に達しています。