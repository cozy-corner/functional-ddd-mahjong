# 麻雀で学ぶ関数型ドメインモデリング - 学習進行TODO

## フェーズ1: Tile型定義 → Hand管理
**学習目標：型安全性とドメイン表現**

### 1.1 基本型定義 ✅
- [x] Tile型の定義（萬子、筒子、索子、字牌）
- [x] 不正な牌（萬子の10など）をコンパイル時に防ぐ設計
- [x] 基本的なTile作成関数とバリデーション

### 1.2 Hand管理 ✅
- [x] Hand型の定義（14牌の制約）
- [x] 手牌の基本操作（追加、削除、ソート）
- [x] Result型を使ったエラーハンドリング

### 1.3 テストとバリデーション ✅
- [x] TileモジュールとHandモジュールの包括的テスト
- [x] パラメータ化テストの実装
- [x] ドメインが仕様であることの実践
- [x] 予測可能な設計への改善

**習得スキル**
- [x] 型駆動設計の実践
- [x] 不変性の活用
- [x] Value Objectパターン
- [x] 基本的なResult型バリデーション
- [x] 関数型プログラミングでのテスト設計

---

## フェーズ2: Meld検出 → Winning hand判定
**学習目標：関数合成とパターンマッチング**

### 2.1 Meld型定義
- [x] 順子（Sequence）型の定義と検出関数
- [x] 刻子（Triplet）型の定義と検出関数
- [x] 雀頭（Pair）型の定義と検出関数

### 2.2 面子分解（小さいPRに分割）

#### PR1: 基本的な面子分解 ✅
- [x] MeldDecompositionモジュールの作成
- [x] tryDecompose関数（14牌→Option<Meld list * Pair>）の基本実装
- [x] Option型を使った面子の存在表現
- [x] シンプルなケースのテスト（明確に4面子1雀頭になるケース）

#### PR2: 完全な面子分解 ✅
- [x] バックトラッキングによる全組み合わせ探索

#### PR3: 複数分解パターン対応 ✅
- [x] 全ての有効な分解パターンを返す関数の実装
- [x] 複数パターンが存在する手牌のテストケース

### 2.3 和了判定 ✅
- [x] 有効な面子組み合わせの検証
- [x] 和了形の判定関数
- [x] テストケース（和了手、ノーテン手）

**習得スキル**
- [x] 関数分解技法
- [x] 合成可能な設計
- [x] パターンマッチングの活用
- [x] Maybe/Option型の活用

---

## フェーズ3: 基本役判定 → 複合役
**学習目標：ドメインロジックの実装とOption型の活用**

### 3.1 基本役の実装（小さいPRに分割）

#### PR1: タンヤオ実装 ✅
- [x] Yaku.fs作成（型定義、エラー型）
- [x] タンヤオ（中張牌のみ）の判定
- [x] タンヤオのテスト実装

#### PR2: 面子構成系の役
- [x] ピンフ（順子4つ+雀頭）の判定
- [x] トイトイ（刻子4つ）の判定

#### PR3: 牌の種類系の役
- [x] ホンイツ（一色+字牌）の判定
- [x] チンイツ（一色のみ）の判定

#### PR4: YakuAnalyzer実装 ✅
- [x] YakuAnalyzer.fs作成（役の合成と分析）
- [x] 複数役の並列判定
- [x] Railway-Oriented Programmingの実践
- [x] 複数役の組み合わせ検証
- [x] 役なし（ノーテン）の処理
- [x] 統合テスト（複数役の組み合わせ）

#### PR5: 全分解パターン対応（予期しない実装）✅
- [x] ピンフ役判定の全分解パターン対応
- [x] トイトイ役判定の全分解パターン対応
- [x] 複数分解パターンを持つ手牌のテストケース追加

#### PR6-1: 一盃口の基本実装
- [x] 一盃口（同じ順子2つ）の役定義追加
- [x] 同じ順子2組の検出ロジック実装
- [x] 一盃口の基本的なテストケース追加
- [x] 全分解パターン対応の確認

#### PR6-2: 高点法の実装 ✅
- [x] Yaku型に翻数（fan）フィールド追加
- [x] 各役の翻数定義（一盃口:1翻、トイトイ:2翻など）
- [x] 相互排他関係の定義（一盃口⇔トイトイ）
- [x] 複数の役組み合わせから最高点選択ロジック実装
- [x] 111222333mRRRGGのような相互排他が発生するケースのテスト
- [x] 高点法の一般的なテストケース追加

---

## フェーズ5: リーチ宣言の実装
**学習目標：Railway-Oriented Programming の実践**

### 5.1 テンパイ判定の実装 ✅
- [x] TenpaiAnalyzer モジュールの作成
- [x] isTenpai関数の実装（13牌からテンパイ判定）
- [x] 待ち牌リストの取得関数
- [x] テンパイ判定のテストケース実装
- [x] パターンベースのテンパイ検出（両面・嵌張・辺張・双碰・単騎）
- [x] 複合パターン対応（1112形、1113形、1112345形）
- [x] 純正九蓮宝燈対応（9種全待ち）
- [x] MeldDecomposition重複除去による性能最適化

### 5.2 リーチドメインの実装 ✅
- [x] ReachContext型の定義（プレイヤー点数、ゲーム段階、リーチ状態）
- [x] ReachError型の定義（ドメイン特化エラー）
- [x] ReachDeclaration型の定義（宣言結果）
- [x] Turn型とScore型のValue Object実装（制約付き）
- [x] 標準Result型による型安全なエラーハンドリング設計

### 5.3 Railway-Oriented Programming の実践 ✅
- [x] リーチ宣言バリデーションパイプラインの実装
- [x] テンパイチェック → 点数チェック → ゲーム段階チェックのResult型チェーン
- [x] bind (>>=) 演算子による関数合成
- [x] パラメーター注入による外部状態管理
- [x] 個別バリデーション関数の実装（checkTenpai, checkScore, checkGameStage, checkReachStatus）
- [x] declareReach メイン関数の実装（Result型チェーン）
- [x] 包括的テストケース実装（成功/失敗シナリオ、パラメータ化テスト）

### 5.4 エラー集約パターンの実装 ✅
- [x] Validation.fsモジュールの作成（エラー集約ユーティリティ）
- [x] sequence関数の実装（Result<'a, 'e> list -> Result<'a list, 'e list>）
- [x] traverse関数の実装（関数適用と結果集約）
- [x] apply関数とApplicative演算子（<*>）の実装
- [x] validateAll関数の実装（並列バリデーション実行）
- [x] declareReachWithAllErrors関数の実装（エラー集約版リーチ宣言）
- [x] 既存declareReach関数の互換性維持（fail-fast方式）
- [x] liftError関数の実装（単一エラー→エラーリスト変換）


**習得スキル**
- [x] **パターンベース分析**: 複数分解パターンの正確な検出
- [x] **関数型アルゴリズム**: 再帰とバックトラッキングの最適化
- [x] **性能改善**: 組み合わせ生成の重複除去（5000パターン→数十パターン）
- [x] **複雑度対応**: 純正九蓮宝燈など最高難度パターンの処理
- [x] **Railway-Oriented Programming の実践**: Result型チェーンによるバリデーションパイプライン
- [x] **Result型チェーンによるエラー伝播**: bind演算子による関数合成
- [x] **ドメイン特化エラーハンドリング**: ReachErrorによる明示的エラー表現
- [x] **パラメーター注入による外部状態管理**: ReachContextによる依存関係処理
- [x] **Applicativeパターンの実装**: 独立したバリデーションの並列実行
- [x] **エラー集約パターンの実装**: 全エラー収集によるUX改善
- [x] **MonadとApplicativeの使い分け**: 依存関係の有無による適切な選択

---

## 完了チェック

### 全体的な習得確認 ✅
- [x] 型安全性：不正な状態をコンパイル時に防げている
- [x] 関数合成：小さな関数を組み合わせて複雑なロジックを構築
- [x] エラーハンドリング：Railway-Oriented Programmingを実践
- [x] ドメイン表現：麻雀のルールがコードに直接表現されている
- [x] テスト：各フェーズで適切なテストケースを作成

---

## フェーズ6: アーキテクチャリファクタリング
**学習目標：関数型アーキテクチャの実践（書籍Chapter 3）**
**設計ドキュメント：[architecture.md](./architecture.md)**

### 6.1 SharedKernelの分離 ✅
- [x] FunctionalDddMahjong.SharedKernelプロジェクト作成
- [x] Validation.fsを移動（汎用的なエラー集約ユーティリティ）
- [x] プロジェクト依存関係の更新（Domain、Testsプロジェクト）
- [x] 全テストの動作確認（308テスト成功）

### 6.2 DomainServicesの分離 ✅
- [x] FunctionalDddMahjong.DomainServicesプロジェクト作成
- [x] 以下のモジュールを移動：
  - [x] TenpaiAnalyzer.fs（テンパイ分析サービス）
  - [x] YakuAnalyzer.fs（役判定サービス）
  - [x] MeldDecomposition.fs（internal化し、Hand.fs経由で公開）
- [x] Domainプロジェクトへの参照設定
- [x] 既存テストの移行と動作確認（76テスト）

### 6.3 Workflowsの分離 ✅
- [x] FunctionalDddMahjong.Workflowsプロジェクト作成
- [x] Reach.fsをReachWorkflow.fsとして移動
- [x] ワークフロー層の責務確認
- [x] 統合テストの実装（Turn, Score, ReachContext等）

### 6.4 プロジェクト構造の最終確認 ✅
- [x] 依存関係の方向性確認（内側に向かう依存）
- [x] 各レイヤーの責務が明確に分離されているか検証
- [x] ソリューションファイルの更新（全プロジェクトの登録）
- [x] 全テストの動作確認（309テスト：233 Domain + 76 DomainServices）

**習得スキル**
- [x] **関数型アーキテクチャ**: Clean Architecture/Onion Architectureの関数型版
- [x] **依存関係管理**: F#プロジェクトにおける適切な参照設定
- [x] **レイヤー分離**: ドメイン、サービス、ワークフローの責務分離
- [x] **モジュール設計**: 高凝集・低結合の実践
- [x] **カプセル化**: internal修飾子による実装詳細の隠蔽

---

## フェーズ7: 境界コンテキストの実装
**学習目標：ドメインと外部世界の境界設計（書籍Chapter 11-12）**

### 7.1 APIレイヤーの実装
**学習目標：型安全な境界設計とデータ変換**

#### PR1: 和了判定API - プロジェクトセットアップとDTO定義
- [x] FunctionalDddMahjong.Apiプロジェクト作成
- [x] プロジェクト参照設定（Domain、DomainServices）
- [x] Dto.fs作成（和了判定用DTO型定義）
  - [x] CheckWinningHandRequest: 和了判定リクエスト（tiles: string list、14枚固定）
  - [x] YakuInfo: 役情報（name, displayName, han, description）
  - [x] CheckWinningHandResponse: 判定結果（isWinningHand, detectedYaku, totalHan）

#### PR2: 和了判定API - 入力変換とバリデーション ✅
- [x] CheckWinningHandHandler.fs作成（parseAndValidateRequest関数）
- [x] 入力バリデーション実装（14枚制約、牌の妥当性、重複制限）
- [x] 単体テスト実装（26テスト）

#### PR3: 和了判定API - ドメイン処理と出力変換
- [ ] CheckWinningHandService.fs作成
- [ ] checkWinningHand関数の実装
  - [ ] 和了判定（Hand.isWinningHand）
  - [ ] 役判定（YakuAnalyzer.analyzeYaku）
  - [ ] 結果をCheckWinningHandResponseに変換
- [ ] 統合テストの実装
  - [ ] 和了手の分析テスト（複数役、高点法適用ケース含む）
  - [ ] ノーテン手の分析テスト

#### PR4: リーチ宣言API
- [ ] リーチ宣言用DTOの追加
  - [ ] DeclareReachRequest: リーチ宣言リクエスト（tiles: string list（13枚）, score, turn）
  - [ ] ReachDeclarationResponse: リーチ宣言結果（canDeclare, reason）
- [ ] ReachDeclarationHandler.fs作成
- [ ] リーチ宣言処理の実装（Workflowsプロジェクトへの参照追加）
- [ ] 統合テストの実装

**習得スキル**
- [ ] **境界コンテキストの設計**: ドメインモデルの保護
- [ ] **DTO パターン**: 外部表現とドメインモデルの分離
- [ ] **双方向変換**: 型安全な変換関数の実装
- [ ] **信頼境界**: 外部データの検証とサニタイゼーション
- [ ] **エラーハンドリング**: 変換失敗の適切な処理

### 7.2 永続化レイヤーの実装
- [ ] FunctionalDddMahjong.Infrastructureプロジェクト作成
- [ ] リポジトリインターフェースの定義
- [ ] インメモリ実装
- [ ] 実データベース対応

---

## 次のステップ（拡張課題）
- [ ] 変形役（七対子、国士無双）の追加
- [ ] ゲーム状態役（リーチ、一発）の追加
- [ ] 符計算の実装
- [ ] 親子概念の追加
- [ ] ドラの実装

