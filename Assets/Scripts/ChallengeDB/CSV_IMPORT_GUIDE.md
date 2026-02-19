# CSV一括インポート ガイド

**ChallengeDB用のCSV一括インポート機能の完全ガイド**

---

## 📋 目次

1. [概要](#概要)
2. [CSVファイルの準備](#csvファイルの準備)
3. [インポート手順](#インポート手順)
4. [エラー対処法](#エラー対処法)
5. [実践例](#実践例)
6. [よくある質問](#よくある質問)

---

## 概要

CSV一括インポート機能を使用すると、大量の問題データ（200問など）を効率的に作成できます。

### 主な特徴

✅ **高速インポート** - 100問を約20秒で作成  
✅ **エラー検出** - 即座にエラー行を特定・表示  
✅ **自動階層化** - type/difficultyでフォルダ自動作成  
✅ **JSON対応** - 複雑な報酬設定も可能  
✅ **キャンセル可能** - プログレスバーからいつでも中断  

### 動作環境

- Unity 6以降
- ChallengeMasterDatabase.asset が作成済み（`Assets/Resources/`）
- ChallengeDBシステムが導入済み

---

## CSVファイルの準備

### ステップ1: CSV形式を理解する

#### 基本構造

CSVファイルは**ヘッダー行 + データ行**で構成されます。

```csv
challengeId,challengeName,description,difficulty,type,questionText,answerType,correctAnswers,hint,rewardItems
challenge_math_001,簡単な足し算,基本的な足し算,Easy,Math,1+1は?,Number,2,指を使おう,"[]"
```

#### 列定義（10列）

| # | 列名 | 必須 | 型 | 説明 | 例 |
|---|------|-----|---|------|-----|
| 1 | challengeId | ✅ | string | 問題の一意なID | `challenge_math_001` |
| 2 | challengeName | ✅ | string | 問題の表示名 | `簡単な足し算` |
| 3 | description | ❌ | string | 問題の説明文 | `基本的な足し算問題です` |
| 4 | difficulty | ❌ | enum | 難易度（デフォルト: Normal） | `Easy`, `Normal`, `Hard` |
| 5 | type | ❌ | enum | 問題の種類（デフォルト: Math） | `Math`, `Logic`, `Quiz` |
| 6 | questionText | ✅ | string | 問題文 | `1 + 1 は?` |
| 7 | answerType | ❌ | enum | 回答タイプ（デフォルト: Text） | `Number`, `Text` |
| 8 | correctAnswers | ✅ | string | 正解（パイプ区切り） | `2` または `東京\|とうきょう` |
| 9 | hint | ❌ | string | ヒント | `指を使って数えよう` |
| 10 | rewardItems | ❌ | JSON | アイテム報酬（JSON配列） | `[]` または後述 |

**必須項目**: challengeId, challengeName, questionText, correctAnswers

---

### ステップ2: 特殊フィールドの記法

#### difficulty（難易度）

以下の5つから選択：

```
Beginner  - 初心者
Easy      - 簡単
Normal    - 普通
Hard      - 難しい
Expert    - 上級者
```

**注意**: 大文字小文字は区別されません（`easy` や `EASY` でもOK）

---

#### type（問題の種類）

以下の5つから選択：

```
Math    - 数学
Logic   - 論理
Memory  - 記憶
Quiz    - クイズ
Other   - その他
```

---

#### answerType（回答タイプ）

```
Number  - 数値入力
Text    - テキスト入力
```

---

#### correctAnswers（正解パターン）

**パイプ（`|`）で区切る**ことで、複数の正解パターンを設定できます。

**例1: 数値問題（単一正解）**
```csv
correctAnswers: 42
```

**例2: テキスト問題（複数正解）**
```csv
correctAnswers: 東京|とうきょう|Tokyo|TOKYO
```

**例3: 数式問題（小数点含む）**
```csv
correctAnswers: 3.14|3.141592
```

---

#### rewardItems（報酬アイテム）

**JSON配列形式**で記述します。

##### 基本構造

```json
[
  {
    "itemId": "アイテムID",
    "minQuantity": 最小個数,
    "maxQuantity": 最大個数,
    "dropRate": ドロップ率（0.0～1.0）
  }
]
```

##### CSV内での記述方法

CSVファイル内では、**ダブルクォートで囲み、内部のダブルクォートを2つ重ねる（`""`）**必要があります。

**例1: 報酬なし**
```csv
rewardItems: []
```

**例2: 単一報酬**
```csv
rewardItems: "[{""itemId"":""gold_coin"",""minQuantity"":10,""maxQuantity"":20,""dropRate"":0.8}]"
```

**例3: 複数報酬**
```csv
rewardItems: "[{""itemId"":""gold_coin"",""minQuantity"":50,""maxQuantity"":100,""dropRate"":0.7},{""itemId"":""rare_gem"",""minQuantity"":1,""maxQuantity"":1,""dropRate"":0.1}]"
```

##### Excelでの編集のコツ

Excel/Google Sheetsでは、セルにJSON文字列を入力すると**自動的にエスケープ**されます。

1. セルに以下を入力:
   ```
   [{"itemId":"gold_coin","minQuantity":10,"maxQuantity":20,"dropRate":0.8}]
   ```
2. CSV保存時に自動的にエスケープされます

---

### ステップ3: サンプルを参考にする

#### 最小限のCSV（必須項目のみ）

```csv
challengeId,challengeName,description,difficulty,type,questionText,answerType,correctAnswers,hint,rewardItems
challenge_test_001,テスト問題,,Normal,Math,1+1は?,Number,2,,[]
```

#### 完全なCSV（全項目設定）

```csv
challengeId,challengeName,description,difficulty,type,questionText,answerType,correctAnswers,hint,rewardItems
challenge_math_easy_001,簡単な足し算,基本的な足し算問題です,Easy,Math,1 + 1 は?,Number,2,指を使って数えてみよう,"[{""itemId"":""gold_coin"",""minQuantity"":5,""maxQuantity"":10,""dropRate"":0.8}]"
```

#### 公式サンプル

詳細なサンプルは以下を参照:
- [Assets/Editor/CSV/challenge_sample.csv](../../Editor/CSV/challenge_sample.csv)

---

### ステップ4: CSVファイルを作成

#### 推奨ツール

**Excel / Google Sheets（推奨）**
- ダブルクォートのエスケープが自動化
- セルの編集が容易
- 数式でID生成などの自動化が可能

**テキストエディタ（VSCode等）**
- 大量データの一括編集に便利
- 正規表現での置換が可能
- エスケープは手動で行う必要あり

#### 文字コード

- **UTF-8推奨**（BOM付きUTF-8も可）
- Shift_JISは文字化けの可能性あり

#### ファイル配置

1. `Assets/Editor/` フォルダを開く（なければ作成）
2. `CSV/` サブフォルダを作成
3. CSVファイルを配置（例: `my_challenges.csv`）

---

## インポート手順

### 手順1: CSV Importerを開く

1. Unityエディタのメニューバーから選択:
   ```
   Tools > Challenge DB > CSV Importer
   ```
2. CSV Importerウィンドウが開きます

![CSV Importer Window]
```
┌─────────────────────────────────┐
│ Challenge CSV Importer          │
├─────────────────────────────────┤
│ CSVファイル: [選択ボックス]     │
│ 出力先: Assets/GameData/Challenges/ │
│                                 │
│ [インポート実行]                │
│                                 │
│ ステータス: 待機中              │
└─────────────────────────────────┘
```

---

### 手順2: CSVファイルを選択

#### 方法A: ドラッグ&ドロップ

1. ProjectウィンドウでCSVファイルを探す
2. **CSVファイル**欄にドラッグ&ドロップ

#### 方法B: 選択ボタン

1. **CSVファイル**欄の右側にある ◎ ボタンをクリック
2. Select TextAssetダイアログでCSVファイルを選択
3. OKをクリック

---

### 手順3: 出力先を確認

**出力先**はデフォルトで `Assets/GameData/Challenges/` です。

変更する場合はパスを直接編集してください。

**フォルダ構造:**
```
Assets/GameData/Challenges/
├── Math/
│   ├── Easy/
│   │   └── challenge_math_easy_001.asset
│   ├── Normal/
│   └── Hard/
├── Logic/
├── Memory/
├── Quiz/
└── Other/
```

`{type}/{difficulty}/` で自動的に階層化されます。

---

### 手順4: インポート実行

1. **「インポート実行」**ボタンをクリック
2. プログレスバーが表示されます

```
CSV Import
Importing 25/100: challenge_math_025
[■■■■■■■□□□□□□□□] 25%
```

**キャンセル可能**: プログレスバーのキャンセルボタンを押すと中断できます

---

### 手順5: 完了確認

#### 成功時

```
┌─────────────────────────┐
│ インポート完了          │
├─────────────────────────┤
│ 成功: 100件 / 100件     │
│ 所要時間: 18.52秒       │
│                         │
│         [OK]            │
└─────────────────────────┘
```

#### キャンセル時

```
┌─────────────────────────┐
│ キャンセル              │
├─────────────────────────┤
│ 25問インポートした      │
│ 時点でキャンセル        │
│ されました。            │
│         [OK]            │
└─────────────────────────┘
```

---

### 手順6: 結果を確認

#### A. ChallengeMasterDatabaseで統計確認

1. `Assets/Resources/ChallengeMasterDatabase` を選択
2. Inspector で **「統計を表示」** ボタンをクリック

```
┌─────────────────────────┐
│ Challenge統計情報       │
├─────────────────────────┤
│ 総問題数: 100          │
│                         │
│ [難易度別]              │
│ Beginner: 10            │
│ Easy: 25                │
│ Normal: 35              │
│ Hard: 20                │
│ Expert: 10              │
│                         │
│ [種類別]                │
│ Math: 40               │
│ Logic: 30              │
│ Quiz: 20               │
│ Memory: 5              │
│ Other: 5               │
│         [OK]            │
└─────────────────────────┘
```

#### B. フォルダ構造を確認

1. Projectウィンドウで `Assets/GameData/Challenges/` を開く
2. 各type/difficultyフォルダに `.asset` ファイルが生成されています

---

## エラー対処法

### エラー時の動作

エラーが発生すると、**即座にインポートが中断**され、詳細なエラーダイアログが表示されます。

```
┌─────────────────────────────┐
│ インポートエラー            │
├─────────────────────────────┤
│ Row 12: challenge_math_012  │
│                             │
│ エラー内容:                 │
│ - questionTextが空です      │
│ - correctAnswersが空です    │
│                             │
│ インポートを中断しました。  │
│ CSVファイルを修正して       │
│ ください。                  │
│          [OK]               │
└─────────────────────────────┘
```

---

### よくあるエラーと対処法

#### 1. CSV構文エラー

**エラーメッセージ:**
```
CSV構文エラー
Line 5: Column count mismatch. Expected 15, got 14
```

**原因:**
- 列数が不足（カンマ忘れ）
- ダブルクォートの閉じ忘れ
- 改行位置の誤り

**対処法:**
1. 該当行（Line 5）を確認
2. カンマの数を数える（データ列は14個必要 = カンマ14個）
3. ダブルクォートのペアを確認
4. セルに改行が含まれていないか確認

**Excelでのデバッグ:**
- CSVをExcelで開き直して列数を確認
- セルに改行が含まれていないか確認（`Ctrl+J` で検索）

---

#### 2. 必須項目欠如

**エラーメッセージ:**
```
Row 8: challenge_logic_008

エラー内容:
- challengeNameが空です
- questionTextが空です
```

**原因:**
必須項目（challengeId, challengeName, questionText, correctAnswers）が空

**対処法:**
1. Row 8の該当セルを確認
2. 必須項目を入力
3. 空白スペースのみの場合も「空」と判定される

**必須チェックリスト:**
- [ ] challengeId が記入済み
- [ ] challengeName が記入済み
- [ ] questionText が記入済み
- [ ] correctAnswers が記入済み

---

#### 3. Enum値不正

**エラーメッセージ:**
```
Row 15: challenge_quiz_015

エラー内容:
- 不正なdifficulty値: Mediam
```

**原因:**
Enum値のスペルミス

**対処法:**
正しいEnum値に修正:

**difficulty:**
- Beginner
- Easy
- Normal
- Hard
- Expert

**type:**
- Math
- Logic
- Memory
- Quiz
- Other

**answerType:**
- Number
- Text

**大小文字は区別されない** ので `easy` や `EASY` でもOKです。

---

#### 4. JSON解析エラー

**エラーメッセージ:**
```
Row 20: challenge_math_020

エラー内容:
- rewardItemsのJSON解析エラー: Invalid JSON
```

**原因:**
- JSON構文エラー（括弧の対応、カンマ忘れ等）
- ダブルクォートのエスケープ不足
- プロパティ名のスペルミス

**対処法:**

**正しいJSON:**
```json
[{"itemId":"gold_coin","minQuantity":10,"maxQuantity":20,"dropRate":0.8}]
```

**CSV内での記述（エスケープ済み）:**
```csv
"[{""itemId"":""gold_coin"",""minQuantity"":10,""maxQuantity"":20,""dropRate"":0.8}]"
```

**検証ツール:**
1. JSON部分をコピー
2. オンラインJSONバリデータ（jsonlint.com等）で検証
3. エラー箇所を修正

**Excelを使う場合:**
- Excelでセルに直接入力すればエスケープは自動

---

#### 5. 画像パスが見つからない

**エラーメッセージ:**
```
Warning: Icon not found: Assets/Icons/math.png
```

**タイプ:** 警告（インポートは継続）

**原因:**
指定したパスに画像アセットが存在しない

**対処法:**
1. パスのスペルミスを確認
2. 画像ファイルがUnityプロジェクトに存在するか確認
3. `Assets/` から始まる相対パスを使用
4. 空にする場合は空文字列またはセルを空欄に

**画像なしでもOK:**
アイコン・問題画像は任意項目なので、警告が出てもインポートは成功します。

---

#### 6. ChallengeMasterDatabaseが見つからない

**エラーメッセージ:**
```
エラー
ChallengeMasterDatabaseが見つかりません。
Assets/Resources/ChallengeMasterDatabase.asset を作成してください。
```

**原因:**
ChallengeMasterDatabase.assetが作成されていない

**対処法:**
1. `Assets/Resources/` フォルダを開く（なければ作成）
2. 右クリック → `Create > Game > Challenge Master Database`
3. 名前を **`ChallengeMasterDatabase`** に変更（必須）
4. 再度インポート実行

---

#### 7. ファイル既存エラー

**エラーメッセージ:**
```
ファイル既存
既に challenge_math_001.asset が存在します。
上書きしますか？

[上書き]  [キャンセル]
```

**原因:**
同じchallengeIdのアセットが既に存在

**対処法:**

**A. 上書きする場合**
- **「上書き」** をクリック
- 既存データは完全に置き換えられます

**B. キャンセルする場合**
- **「キャンセル」** をクリック
- インポートが中断されます
- CSVのchallengeIdを別の値に変更してから再実行

**推奨:**
- テストデータの場合: 上書きOK
- 本番データの場合: challengeIdの重複を避ける

---

## 実践例

### 例1: 数学問題100問を作成

#### CSVファイル構成

```csv
challengeId,challengeName,description,difficulty,type,questionText,answerType,correctAnswers,hint,rewardItems
challenge_math_001,足し算1,,Beginner,Math,2 + 3 は?,Number,5,,"[{""itemId"":""coin"",""minQuantity"":5,""maxQuantity"":10,""dropRate"":1.0}]"
challenge_math_002,足し算2,,Beginner,Math,5 + 8 は?,Number,13,,"[{""itemId"":""coin"",""minQuantity"":5,""maxQuantity"":10,""dropRate"":1.0}]"
...（98行省略）
challenge_math_100,因数分解,高度な数学,Expert,Math,x^2-7x+12を因数分解せよ,Text,(x-3)(x-4),因数分解の公式を使う,"[{""itemId"":""rare_gem"",""minQuantity"":1,""maxQuantity"":1,""dropRate"":0.2}]"
```

#### インポート結果

```
Assets/GameData/Challenges/
└── Math/
    ├── Beginner/   (20問)
    ├── Easy/       (30問)
    ├── Normal/     (30問)
    ├── Hard/       (15問)
    └── Expert/     (5問)
```

---

### 例2: クイズ50問を作成

#### CSVファイル構成（一部）

```csv
challengeId,challengeName,description,difficulty,type,questionText,answerType,correctAnswers,hint,rewardItems
challenge_quiz_001,首都クイズ,世界の首都,Easy,Quiz,フランスの首都は?,Text,パリ|Paris,ヨーロッパの都市,"[]"
challenge_quiz_002,歴史クイズ,日本史,Normal,Quiz,鎌倉幕府を開いたのは?,Text,源頼朝|みなもとのよりとも|頼朝,,"[{""itemId"":""book"",""minQuantity"":1,""maxQuantity"":1,""dropRate"":0.5}]"
```

**ポイント:**
- `correctAnswers` にパイプ区切りで複数正解を設定
- テキスト問題は常に大小文字を区別しません

---

### 例3: Excelで効率的に作成

#### Excelでの作業フロー

1. **テンプレート作成**
   - ヘッダー行を入力
   - 1行目にサンプルデータを入力

2. **数式で自動化**
   ```excel
   A2: ="challenge_math_"&TEXT(ROW()-1,"000")
   B2: ="数学問題"&(ROW()-1)
   ```

3. **オートフィル**
   - 数式をドラッグして100行コピー

4. **CSV保存**
   - 「名前を付けて保存」→ 「CSV UTF-8」

5. **Unityでインポート**

---

## よくある質問

### Q1: CSVファイルの文字コードは何が良い？

**A:** UTF-8（BOM付きでもOK）を推奨。

- Excel: 「CSV UTF-8」で保存
- Google Sheets: ダウンロード時に自動的にUTF-8
- VSCode/Notepad++: UTF-8で保存

Shift_JISは文字化けのリスクがあります。

---

### Q2: rewardItemsを空にする方法は？

**A:** 以下のいずれかを使用:

```csv
rewardItems: []
rewardItems: ""
rewardItems: (空セル)
```

すべて「報酬なし」として扱われます。

---

### Q3: correctAnswersに改行を含めたい

**A:** CSV規格上、セル内改行は制限があるため**非推奨**です。

**代替案:**
- `\n` を含む文字列を設定
- ゲーム側で `questionText.Replace("\\n", "\n")` で置換

---

### Q4: 一度に何問までインポートできる？

**A:** 理論上は制限なしですが、実用的には:

- **推奨**: 100問ずつ
- **最大**: 500問程度

大量データの場合、分割インポートをお勧めします。

---

### Q5: エラーが出た行だけスキップできる？

**A:** 現在の実装では**即座に中断**します。

**理由**: エラーデータを早期に発見し、修正を促すため

**対処法**:
1. エラー行を修正
2. 修正後、再度インポート実行
3. 既存データは「上書き」または「スキップ」を選択

---

### Q6: 既存のChallengeDataアセットをCSVにエクスポートできる？

**A:** 現在はエクスポート機能は未実装です。

**将来の拡張予定**として検討中です。

---

### Q7: インポート後、ChallengeMasterDatabaseに自動登録される？

**A:** はい。インポート処理の中で自動的に `ChallengeMasterDatabase.Instance.AddChallengeData()` が呼ばれます。

手動での登録は不要です。

---

### Q8: プログレスバーの途中でキャンセルしたらどうなる？

**A:** キャンセル時点までのデータは**保存されます**。

例: 100問中50問でキャンセル → 50問分のアセットが作成済み

再実行時、重複IDは「上書き確認ダイアログ」が表示されます。

---

### Q9: Excelで編集してもJSON部分が壊れない？

**A:** Excelは自動的にエスケープ処理を行うため、通常は問題ありません。

**注意点:**
- セルに直接入力する（数式として入力しない）
- ダブルクォートで囲まずに入力（Excelが自動処理）

---

## パフォーマンスガイド

### 処理時間の目安

| 問題数 | 所要時間 | 備考 |
|-------|----------|------|
| 10問 | 1-3秒 | テスト向け |
| 50問 | 5-10秒 | 中規模データ |
| 100問 | 10-20秒 | 推奨サイズ |
| 200問 | 20-40秒 | 大規模データ |
| 500問 | 50-100秒 | 最大推奨 |

**要因:**
- アセット作成・保存の時間
- バリデーション処理
- フォルダ作成処理

---

### 最適化のコツ

1. **分割インポート**
   - 100問ずつに分けてインポート
   - エラー発生時のデバッグが容易

2. **自動保存を活用**
   - 100問ごとに自動保存される
   - キャンセルしてもデータロストなし

---

## トラブルシューティング

### プログレスバーが進まない

**原因:** 大量データ処理中

**対処法:** 
- しばらく待つ（500問で最大2分程度）
- キャンセルして分割インポートに切り替え

---

### インポート後、問題が表示されない

**チェック項目:**
1. ChallengeMasterDatabaseを選択して統計確認
2. `Assets/GameData/Challenges/` にアセットが存在するか確認
3. Console ウィンドウでエラーログを確認

---

### CSVファイルが選択できない

**原因:** TextAssetとして認識されていない

**対処法:**
1. `.csv` 拡張子を確認
2. Unity エディタでプロジェクトをRefresh (Ctrl+R)
3. CSVファイルを `Assets/` 以下に配置

---

## まとめ

CSV一括インポート機能を使えば、**200問を30秒程度**で効率的に作成できます。

### 基本フロー

1. ✅ CSVファイルを作成（Excel推奨）
2. ✅ `Assets/Editor/CSV/` に配置
3. ✅ `Tools > Challenge DB > CSV Importer` を開く
4. ✅ CSVファイルを選択
5. ✅ 「インポート実行」をクリック
6. ✅ 統計で確認

### 成功のためのポイント

- 📝 サンプルCSVを参考にする
- 🧪 小規模テストから始める
- 🔍 エラーメッセージをよく読む
- 📊 Excel活用で効率化
- 💾 バックアップを忘れずに

---

**関連ドキュメント:**
- [README.md](README.md) - ChallengeDBシステム全体のドキュメント
- [challenge_sample.csv](../../Editor/CSV/challenge_sample.csv) - サンプルCSVファイル

**作成日**: 2026年2月19日  
**バージョン**: v1.1.0
