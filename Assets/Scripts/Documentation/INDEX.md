# Facility自動生成機能 - ドキュメント索引

**最終更新**: 2026年2月12日

## ドキュメント一覧

このフォルダには、Facility自動生成機能に関する以下のドキュメントがあります。

---

### � FACILITY_SETUP_GUIDE.md ⭐ NEW!
**対象**: 初めて設定する方（最優先）  
**内容**: 実際のセットアップ手順

- ItemData の作成と登録
- FacilityData の設定（Inspector）
- 解放スクリプトの作成
- トラブルシューティング
- デバッグ方法

👉 **設備を追加・設定する方はここから読んでください**

---

### 📘 FACILITY_PRODUCTION_USAGE_GUIDE.md
**対象**: すべてのユーザー（推奨）  
**内容**: 自動生成機能の使い方ガイド（TimerManager統合版）

- クイックスタート（5分で開始）
- セットアップ手順
- 基本的な使い方
- TimerManagerとの連携方法
- 実践例（コード付き）
- トラブルシューティング
- FAQ

👉 **機能の使い方を知りたい方向け**

---

### 📗 README_FacilityProduction.md
**対象**: 開発者  
**内容**: 技術仕様書

- データ構造の詳細
- コード例（詳細）
- カスタマイズ方法
- 技術的な実装詳細

👉 **機能を拡張したい開発者向け**

---

### 📕 TIMER_INTEGRATION_GUIDE.md
**対象**: 開発者（システム統合担当者）  
**内容**: TimerManager統合完了報告書

- 統合内容の詳細
- 実装されたコードの解説
- 動作仕様
- テスト手順
- トラブルシューティング
- 今後の作業

👉 **TimerManager統合の技術詳細を知りたい開発者向け**

---

## 推奨される読む順序

### 初めて設備を追加する場合
1. **FACILITY_SETUP_GUIDE.md** ⭐ - 設定手順に従う
2. **FACILITY_PRODUCTION_USAGE_GUIDE.md** - より詳しい使い方を確認

### 初めて使う場合
1. **FACILITY_PRODUCTION_USAGE_GUIDE.md** - 使い方を理解
2. **README_FacilityProduction.md** - より詳しい機能を確認

### 開発・カスタマイズする場合
1. **FACILITY_PRODUCTION_USAGE_GUIDE.md** - 基本を理解
2. **README_FacilityProduction.md** - 技術仕様を確認
3. **TIMER_INTEGRATION_GUIDE.md** - TimerManager統合の詳細を確認

### TimerManager統合について知りたい場合
1. **TIMER_INTEGRATION_GUIDE.md** - 統合詳細を確認

---

## クイックリンク

### セットアップ
- [ItemDataの作成方法](FACILITY_SETUP_GUIDE.md#ステップ1-アイテムデータの作成)
- [FacilityDataの設定](FACILITY_SETUP_GUIDE.md#ステップ2-facilitydata-の設定)
- [解放スクリプトの作成](FACILITY_SETUP_GUIDE.md#ステップ3-解放スクリプトの作成)

### よくある質問
- [施設が生成しない場合](FACILITY_SETUP_GUIDE.md#問題1-生成されない) 
- [アイテムが追加されない](FACILITY_SETUP_GUIDE.md#問題2-アイテムが追加されない)
- [お金が増えない](FACILITY_SETUP_GUIDE.md#問題3-お金が増えない)
- [生成間隔がおかしい場合](FACILITY_PRODUCTION_USAGE_GUIDE.md#生成間隔がおかしい)
- [デバッグログが表示されない](FACILITY_PRODUCTION_USAGE_GUIDE.md#デバッグログが表示されない)

### コード例
- [クイックスタート](FACILITY_PRODUCTION_USAGE_GUIDE.md#クイックスタート)
- [実践例](FACILITY_PRODUCTION_USAGE_GUIDE.md#実践例)
- [詳細なコード例](README_FacilityProduction.md#実装例)

### 技術情報
- [TimerManager統合詳細](TIMER_INTEGRATION_GUIDE.md#統合内容の詳細)
- [データ構造リファレンス](FACILITY_PRODUCTION_USAGE_GUIDE.md#データ構造リファレンス)
- [トラブルシューティング](TIMER_INTEGRATION_GUIDE.md#注意事項)

---

## 機能概要

### 自動生成機能とは
Facility（施設）が一定間隔で自動的にお金やアイテムを生成する機能です。

### 主な特徴
- ✅ お金、アイテム、または両方を自動生成
- ✅ TimerManagerと完全統合（ゲーム時間ベース）
- ✅ タイマー一時停止時は生成も自動停止
- ✅ タイマー終了時に統計を自動表示
- ✅ フォールバック機能（TimerManagerなしでも動作）
- ✅ 生成統計の自動記録

### 統合ステータス
| 機能 | ステータス |
|------|-----------|
| 基本生成機能 | ✅ 完了 |
| TimerManager統合 | ✅ 完了（2026/02/12） |
| GameDatabase連携 | ⏳ TODO |

---

## サポート

### バグ報告・質問
プロジェクト管理者までお問い合わせください。

### ドキュメントの改善提案
ドキュメントの改善提案も歓迎します。

---

**Happy Coding! 🎮**
