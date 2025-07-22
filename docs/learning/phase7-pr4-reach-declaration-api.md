# Phase 7 PR4: リーチ宣言API実装から学んだこと

## 概要
既存のリーチ宣言ドメインロジック（ReachWorkflow）を活用して、APIエンドポイントを実装しました。

## 主な学び

### 1. API設計の明確性
**課題**: ハンドラーでpublic関数が複数あり、どれがエンドポイントか不明確
**解決**: 
- メインエンドポイント関数（`handleDeclareReach`）のみをpublic
- ヘルパー関数（`parseTile`, `parseAndValidateRequest`）をprivate
- 単一責任の原則で明確なAPI設計

```fsharp
module ReachDeclarationHandler =
    // private関数（実装詳細）
    let private parseTile (tileStr: string) : Result<Tile.Tile, string> = ...
    let private parseAndValidateRequest (request: DeclareReachRequest) = ...
    
    // public関数（APIエンドポイント）
    let handleDeclareReach (request: DeclareReachRequest) : Result<DeclareReachResponse, string> = ...
```

### 2. 責務の適切な分離
**3層の責務分割**:
1. **APIハンドラー**: リクエスト検証・変換
2. **ApplicationService**: ドメインロジック呼び出し・エラーメッセージ変換
3. **Workflow**: ビジネスロジック実行

### 3. DTOの役割設計
**役の得点は返さない**: リーチ宣言APIはリーチの成否のみを責務とし、役の得点計算は別APIに委ねる設計

```fsharp
type DeclareReachResponse =
    {
        success: bool                    // 宣言成功/失敗
        reachType: string option         // "Reach" or "DoubleReach"
        error: string option             // エラーメッセージ（日本語）
        // han: int option               // ❌ 削除 - 役の責務ではない
    }
```

### 4. 既存ドメインロジックの活用
**ReachWorkflowの型構造**:
- Value Objects（`Score`, `Turn`）の型安全性
- Railway-Oriented Programmingパターン
- ドメイン特化エラー（`ReachError`）

既存の堅牢なドメインロジックをそのまま活用できました。

### 5. エラーハンドリングの一貫性
**2段階のエラーハンドリング**:
1. **バリデーションエラー**: `Result<_, string>`（API層）
2. **ドメインエラー**: `Result<_, ReachError>`（ドメイン層）

```fsharp
// API層でのバリデーション
| Error msg -> Result.Error msg  // "Invalid tile: 10m"

// ドメイン層でのビジネスエラー  
| Error ReachError.NotTenpai -> "手牌が聴牌していません"
```

### 6. テスト戦略の改善
**private関数のテスト問題**:
- 当初: private関数を直接テスト → コンパイルエラー
- 改善: public APIを通した統合テスト

**テストの焦点**:
- エンドポイントの振る舞い
- エラーメッセージの正確性
- ドメインルールの遵守

### 7. 依存関係管理
**プロジェクト参照の追加**:
```xml
<!-- ApplicationServices.fsproj -->
<ProjectReference Include="..\FunctionalDddMahjong.Workflows\..." />
```

**open文の最適化**:
```fsharp
// 不要なopenを削除
open FunctionalDddMahjong.Domain      // ✅ Hand.Hand
open FunctionalDddMahjong.Workflows   // ✅ ReachError, Score, etc.
// open FunctionalDddMahjong.SharedKernel    // ❌ 未使用
// open FunctionalDddMahjong.DomainServices  // ❌ 未使用
```

## 技術的改善点

### 1. コードの可読性
- 関数の責務を明確化
- 型名の一貫性（Request/Response）
- 日本語エラーメッセージでUX向上

### 2. 型安全性
- DTO→ドメインモデル変換での型チェック
- Value Objectによる制約の保証
- Result型による明示的エラーハンドリング

### 3. 保守性
- 単一責任の原則
- 疎結合な設計
- 既存コードの再利用

## 次のPRへの示唆

1. **一貫したAPI設計**: 他のAPIでも同じパターンを適用
2. **テスト戦略**: public APIを通した統合テスト中心
3. **エラーメッセージ**: ユーザーフレンドリーな日本語メッセージ
4. **依存関係**: 必要最小限のopenとプロジェクト参照

## まとめ
既存のドメインロジックが堅牢だったため、API層の追加がスムーズに実現できました。責務の分離と型安全性を保ちながら、実用的なAPIを構築する関数型アプローチを実践できました。