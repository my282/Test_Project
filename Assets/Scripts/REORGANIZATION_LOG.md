# Scripts ディレクトリ整理ログ

**整理日時**: 2026年2月10日

## 概要
Scriptsディレクトリ直下に散在していたファイルを、機能別のサブディレクトリに整理しました。

## 新規作成ディレクトリ
- `Database/` - データベース関連スクリプト
- `Facility/` - 施設関連スクリプト
- `Item/` - アイテム関連スクリプト
- `Documentation/` - ドキュメントファイル

## ファイル移動の詳細

### Database/ に移動したファイル
- `DatabaseExample.cs` + `.meta`
- `GameDatabase.cs` + `.meta`
- `MasterDatabase.cs` + `.meta`
- `MasterDatabaseExample.cs` + `.meta`

**説明**: データベース関連の基本機能とサンプル実装

### Facility/ に移動したファイル
- `Facility.cs` + `.meta`
- `FacilityCostDemo.cs` + `.meta`
- `FacilityData.cs` + `.meta`
- `FacilityProduction.cs` + `.meta`
- `FacilityProductionController.cs` + `.meta`

**説明**: 施設の定義、データ、生産機能、コスト計算デモなど

### Item/ に移動したファイル
- `Item.cs` + `.meta`
- `ItemCost.cs` + `.meta`
- `ItemData.cs` + `.meta`

**説明**: アイテムの定義、コスト情報、データ構造など

### Documentation/ に移動したファイル
- `README_FacilityProduction.md` + `.meta`
- `TIMER_INTEGRATION_GUIDE.md` + `.meta`

**説明**: 機能説明とインテグレーションガイド

## 既存のディレクトリ
以下のディレクトリは既に存在していたため、そのまま維持しています:
- `Editor/` - エディタ拡張スクリプト
- `HubPage/` - ハブページ関連
- `Persistent/` - 永続化関連

## 整理後のディレクトリ構造

```
Assets/Scripts/
├── Database/         # [新規] データベース関連
├── Facility/         # [新規] 施設関連
├── Item/             # [新規] アイテム関連
├── Documentation/    # [新規] ドキュメント
├── Editor/           # [既存] エディタ拡張
├── HubPage/          # [既存] ハブページ
└── Persistent/       # [既存] 永続化機能
```

## 注意事項
- すべての `.meta` ファイルも対応するスクリプトと一緒に移動しています
- Unity のプロジェクトでは `.meta` ファイルの維持が重要です
- スクリプト参照は Unity が自動的に更新するため、手動での変更は不要です
