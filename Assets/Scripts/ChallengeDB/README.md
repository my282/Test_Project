# Challenge Database システム

**入力式問題システムの完全実装**

## 📋 目次

- [概要](#概要)
- [システム構成](#システム構成)
- [クイックスタート](#クイックスタート)
- [詳細ガイド](#詳細ガイド)
- [CSV一括インポート](#csv一括インポート)
- [GameDatabase統合（将来の拡張用）](#gamedatabase統合将来の拡張用)
- [API リファレンス](#api-リファレンス)
- [更新履歴](#更新履歴)

---

## 概要

Challenge Databaseシステムは、入力式問題を管理・実行するためのフレームワークです。以下の機能を提供します：

### 主な機能

✅ **入力式問題対応** - ユーザーがテキストや数値を入力して回答  
✅ **5段階難易度** - Beginner～Expertの難易度設定  
✅ **ランダム出題** - 難易度別にランダムで問題を選択  
✅ **確率報酬システム** - ガチャ形式の報酬抽選  
✅ **複数正解パターン** - 1つの問題に複数の正解を設定可能  
✅ **数値許容範囲** - 小数点の誤差を考慮した判定  
✅ **大小文字設定** - テキスト回答の大小文字区別/無視  
✅ **進行状態管理** - 挑戦回数、クリア回数、最終プレイ日時を記録

### アーキテクチャ

既存のItem/Facilityシステムと同じ**二層構造**を採用：

```
┌───────────────────────────────────────┐
│ ChallengeMasterDatabase (ScriptableObject) │  ← 全問題の管理
│ - Resources/ChallengeMasterDatabase.asset  │
│ - 200問を一元管理                          │
│ - 統計機能、検索機能                       │
└───────────────────────────────────────┘
              ↓ 管理
┌─────────────────────────────────┐
│  ChallengeData (ScriptableObject) │  ← 個別問題のマスターデータ
│  - 問題文、正解パターン          │
│  - 報酬テーブル                 │
│  - 難易度、種類                 │
└─────────────────────────────────┘
            ↓ CreateChallenge()
┌─────────────────────────────────┐
│  Challenge (Serializable)        │  ← ランタイムデータ（動的）
│  - プレイヤーの進行状態          │
│  - 挑戦回数、クリア回数          │
│  - 回答チェック処理              │
└─────────────────────────────────┘
```

---

## システム構成

### ファイル一覧

#### コアシステム

| ファイル名 | 役割 | 説明 |
|-----------|------|------|
| `ChallengeEnums.cs` | Enum定義 | 難易度、種類、状態などの列挙型 |
| `RewardSystem.cs` | 報酬システム | 確率ベースの報酬抽選ロジック |
| `ChallengeData.cs` | マスターデータ | ScriptableObject、問題の定義 |
| `Challenge.cs` | ランタイムデータ | プレイヤーの進行状況を管理 |
| `ChallengeMasterDatabase.cs` | **マスターDB** | **全問題データの一元管理** |
| `ChallengeDatabaseHelper.cs` | ヘルパー | ユーティリティ関数集 |
| `ChallengeDatabaseExample.cs` | デモ | 使用例とテストスクリプト |

#### エディタ拡張

| ファイル名 | 役割 | 説明 |
|-----------|------|------|
| `Editor/ChallengeDataEditor.cs` | カスタムInspector | 問題作成を効率化 |
| `Editor/ChallengeMasterDatabaseEditor.cs` | カスタムInspector | MasterDBの統計表示 |
| `Editor/CSVSimpleParser.cs` | CSVパーサー | RFC 4180準拠のCSV解析 |
| `Editor/ChallengeCSVParser.cs` | CSV変換 | CSV行をChallengeDataに変換 |
| `Editor/ChallengeCSVImporter.cs` | EditorWindow | CSV一括インポートUI |

### Enum定義

#### ChallengeDifficulty（難易度）
```csharp
public enum ChallengeDifficulty
{
    Beginner,   // 初心者
    Easy,       // 簡単
    Normal,     // 普通
    Hard,       // 難しい
    Expert      // 上級者
}
```

#### ChallengeType（種類）
```csharp
public enum ChallengeType
{
    Math,       // 数学
    Logic,      // 論理
    Memory,     // 記憶
    Quiz,       // クイズ
    Other       // その他
}
```

#### AnswerType（回答タイプ）
```csharp
public enum AnswerType
{
    Number,     // 数値入力
    Text        // テキスト入力
}
```

#### ChallengeStatus（状態）
```csharp
public enum ChallengeStatus
{
    Locked,     // ロック中
    Unlocked,   // 解放済み
    Completed   // クリア済み
}
```

---

## クイックスタート

### 0. 初期セットアップ（最初の1回だけ）

**ChallengeMasterDatabaseアセットを作成:**

1. `Assets/Resources/` フォルダを開く（なければ作成）
2. 右クリック → `Create > Game > Challenge Master Database`
3. 名前を **`ChallengeMasterDatabase`** に変更（重要！）
4. 完了！これで問題管理の準備完了

> 💡 このアセットは**1つだけ**作成してください。全ての問題データがここに登録されます。

### 1. 問題データの作成

1. Projectウィンドウで右クリック
2. `Create > Game > Challenge Data` を選択
3. 問題データに名前をつける（例: `Challenge_Math_001`）

### 2. 問題データの設定

Inspectorで以下を設定：

```
【基本情報】
- Challenge Id: "challenge_math_001"
- Challenge Name: "簡単な足し算"
- Description: "2つの数字を足した答えを入力してください"
- Difficulty: Easy
- Type: Math

【問題内容】
- Question Text: "3 + 5 = ?"
- Answer Type: Number
- Correct Answers: ["8"]
- Numeric Tolerance: 0

【報酬設定】
- Guaranteed Money: 100
- Reward Items:
  - Item Id: "item_wood"
  - Min Quantity: 1
  - Max Quantity: 3
  - Drop Rate: 0.5
```

### 3. ChallengeMasterDatabaseに登録

問題データを作成したら、MasterDatabaseに登録します：

1. 作成した問題データを選択
2. Inspector下部にある **「MasterDBに追加」** ボタンをクリック
3. Console に「問題データを追加しました」と表示されればOK！

### 4. 登録確認

1. `Assets/Resources/ChallengeMasterDatabase` アセットを選択
2. Inspectorで登録した問題が表示されます
3. **「統計を表示」** ボタンで問題数を確認できます

### 5. テスト実行（オプション）

1. シーンに空のGameObjectを作成
2. `ChallengeDatabaseExample` コンポーネントをアタッチ
3. `Test Challenges` リストに作成した問題データを追加
4. コンポーネントを右クリック → コンテキストメニューからテスト実行

```
1. ランダムに問題を選択
2. 回答をチェック（Test Answerに回答を入力）
3. 報酬抽選テスト
4. 統計情報を表示
```

---

## 詳細ガイド

### 入力式問題の設計

#### 数値問題の例

```
問題: "円周率の値は？（小数第2位まで）"
Answer Type: Number
Correct Answers: ["3.14"]
Numeric Tolerance: 0.01  // ±0.01の誤差を許容
```

この設定では、`3.13`～`3.15`の範囲が正解として扱われます。

#### テキスト問題の例

```
問題: "日本の首都はどこですか？"
Answer Type: Text
Correct Answers: ["東京", "Tokyo", "とうきょう"]
Case Sensitive: false  // 大小文字を区別しない
```

複数の正解パターンを設定することで、柔軟な判定が可能です。

### 報酬システムの設計

#### 固定報酬

```csharp
Guaranteed Money: 100  // 必ず100円獲得
```

#### 確率報酬

```csharp
Reward Items:
  - Item: "rare_item"
    Min Quantity: 1
    Max Quantity: 1
    Drop Rate: 0.1  // 10%の確率でドロップ
    
  - Item: "common_item"
    Min Quantity: 3
    Max Quantity: 5
    Drop Rate: 0.7  // 70%の確率で3～5個ドロップ
```

**ドロップ率の合計が1.0を超えても問題ありません**。各アイテムは独立して抽選されます。

### ヘルパー関数の使い方

#### ランダムに問題を選択

```csharp
List<ChallengeData> allChallenges = /* ... */;
ChallengeDifficulty difficulty = ChallengeDifficulty.Normal;

ChallengeData randomChallenge = ChallengeDatabaseHelper.GetRandomChallengeByDifficulty(
    allChallenges, 
    difficulty
);

if (randomChallenge != null)
{
    Challenge challenge = randomChallenge.CreateChallenge();
    challenge.Unlock();
    // 問題を表示...
}
```

#### 回答チェック

```csharp
Challenge currentChallenge = /* ... */;
string userInput = "答え";

currentChallenge.IncrementAttempt();
bool isCorrect = currentChallenge.CheckAnswer(userInput);

if (isCorrect)
{
    currentChallenge.RecordCompletion();
    // 報酬を付与...
}
```

#### 報酬付与

```csharp
ChallengeData challengeData = /* ... */;

ChallengeDatabaseHelper.GiveRewards(
    challengeData.rewardTable,
    onItemReward: (itemId, quantity) =>
    {
        // GameDatabase.Instance.AddItem(itemId, ...);
        Debug.Log($"アイテム {itemId} x {quantity} 獲得！");
    },
    onMoneyReward: (amount) =>
    {
        // GameDatabase.Instance.AddMoney(amount);
        Debug.Log($"お金 {amount} 獲得！");
    }
);
```

#### 統計情報の取得

```csharp
List<Challenge> playerChallenges = /* ... */;

// 全体統計
string stats = ChallengeDatabaseHelper.GetStatistics(playerChallenges);
Debug.Log(stats);

// クリア済み数
int completedCount = ChallengeDatabaseHelper.GetCompletedChallengeCount(playerChallenges);

// 難易度別クリア数
int hardCompleted = ChallengeDatabaseHelper.GetCompletedChallengeCountByDifficulty(
    playerChallenges, 
    ChallengeDifficulty.Hard
);
```

### ChallengeMasterDatabaseの使い方

#### シングルトンアクセス

```csharp
// どこからでもアクセス可能（GameObjectにアタッチ不要）
ChallengeMasterDatabase masterDB = ChallengeMasterDatabase.Instance;
```

#### 問題データの取得

```csharp
// すべての問題を取得
List<ChallengeData> allChallenges = ChallengeMasterDatabase.Instance.GetAllChallengeData();

// IDで問題を検索
ChallengeData challenge = ChallengeMasterDatabase.Instance.GetChallengeData("challenge_math_001");

// 難易度別に取得
List<ChallengeData> easyChallenges = ChallengeMasterDatabase.Instance.GetChallengeDataByDifficulty(
    ChallengeDifficulty.Easy
);

// ランダムに問題を選択
ChallengeData randomChallenge = ChallengeMasterDatabase.Instance.GetRandomChallengeByDifficulty(
    ChallengeDifficulty.Normal
);
```

#### 統計情報の取得

```csharp
// 総問題数
int totalCount = ChallengeMasterDatabase.Instance.GetChallengeCount();
Debug.Log($"登録済み問題数: {totalCount}");

// 難易度別の問題数
int normalCount = ChallengeMasterDatabase.Instance.GetChallengeCountByDifficulty(
    ChallengeDifficulty.Normal
);

// 統計情報を表示
string stats = ChallengeMasterDatabase.Instance.GetStatistics();
Debug.Log(stats);
```

#### エディタ機能

ChallengeMasterDatabaseアセットを選択すると、Inspector に以下が表示されます：

- **統計を表示** ボタン - 全問題の統計をダイアログで表示
- **クイック情報** - 難易度別・種類別の問題数を一目で確認
- **すべて削除** ボタン - 全問題データをクリア（要確認）
- **保存** ボタン - 変更を保存

### カスタムInspectorの機能

#### ChallengeDataEditor（問題作成用）

問題データを選択すると、Inspector に以下のボタンが表示されます：

1. **ID自動生成** - `challenge_[種類]_[難易度]_[連番]` 形式で自動生成
2. **バリデーション** - データの妥当性をチェック
3. **複製して新規作成** - 似た問題を作る際に便利
4. **MasterDBに追加** - ChallengeMasterDatabaseに登録

**折りたたみ可能なセクション:**
- **問題プレビュー** - 問題文、正解、設定を見やすく表示
- **報酬情報** - ドロップ率の合計と期待値を自動計算

---

## CSV一括インポート

大量の問題データ（200問など）を効率的に作成するためのCSVインポート機能を提供しています

### CSVファイルの準備

#### CSV形式仕様

**列定義（10列）:**
```csv
challengeId,challengeName,description,difficulty,type,questionText,answerType,correctAnswers,hint,rewardItems
```

#### 特殊フィールドの記法

| フィールド | 形式 | 例 |
|----------|------|-----|
| `correctAnswers` | パイプ区切り | `42\|42.0\|四十二` |
| `rewardItems` | **JSON配列** | `[{"itemId":"gold","minQuantity":10,"maxQuantity":20,"dropRate":0.8}]` |
| `difficulty` | Enum文字列 | `Beginner`, `Easy`, `Normal`, `Hard`, `Expert` |
| `type` | Enum文字列 | `Math`, `Logic`, `Memory`, `Quiz`, `Other` |
| `answerType` | Enum文字列 | `Number`, `Text` |

#### RewardItems（JSON配列）の記法

空の場合:
```csv
[]
```

1つの報酬:
```csv
"[{""itemId"":""gold_coin"",""minQuantity"":10,""maxQuantity"":20,""dropRate"":0.8}]"
```

複数の報酬:
```csv
"[{""itemId"":""gold_coin"",""minQuantity"":10,""maxQuantity"":20,""dropRate"":0.8},{""itemId"":""rare_gem"",""minQuantity"":1,""maxQuantity"":1,""dropRate"":0.1}]"
```

**注意点:**
- CSV内のJSON文字列全体をダブルクォート `"` で囲む
- JSON内部のダブルクォートは `""` にエスケープ（2つ重ねる）
- ExcelでCSVを編集する場合、自動的にエスケープされます

#### サンプルCSV

[Assets/Editor/CSV/challenge_sample.csv](../../../Editor/CSV/challenge_sample.csv) に10問分のサンプルがあります。参考にしてください。

**サンプル行（見やすく整形）:**
```csv
challengeId: challenge_math_easy_001
challengeName: 簡単な足し算
description: 基本的な足し算問題です
difficulty: Easy
type: Math
questionText: 1 + 1 は?
answerType: Number
correctAnswers: 2
hint: 指を使って数えてみよう
rewardItems: "[{""itemId"":""gold_coin"",""minQuantity"":5,""maxQuantity"":10,""dropRate"":0.8}]"
```

### インポート手順

#### 1. CSVファイルをプロジェクトに配置

1. CSVファイルを作成（Excelや任意のテキストエディタで）
2. `Assets/Editor/CSV/` フォルダに配置（フォルダがなければ作成）
3. Unity エディタで認識されるまで待つ

#### 2. CSV Importerを開く

1. Unity メニューバーから `Tools > Challenge DB > CSV Importer` を選択
2. CSV Importer ウィンドウが開きます

#### 3. CSVファイルを選択

1. **CSVファイル** 欄に、作成したTextAssetをドラッグ&ドロップ
   - または、◎ボタンをクリックして `challenge_sample.csv` などを選択
2. **出力先** はデフォルトの `Assets/GameData/Challenges/` のままでOK

#### 4. インポート実行

1. **「インポート実行」** ボタンをクリック
2. プログレスバーが表示されます（キャンセル可能）
3. 完了ダイアログが表示されたら成功です

```
インポート完了
成功: 10件 / 10件
所要時間: 2.35秒

[OK]
```

#### 5. 確認

1. `Assets/GameData/Challenges/` フォルダを確認
   - `Math/Easy/challenge_math_easy_001.asset` のように階層化されて保存
2. `Assets/Resources/ChallengeMasterDatabase` を選択
   - Inspectorで「統計を表示」ボタンをクリック
   - インポートした問題数が反映されているか確認

### エラーハンドリング

#### エラー時の動作

**即座に中断方式**を採用しています：
- エラーが発生した行で即座にインポート停止
- 詳細なエラーメッセージをダイアログ表示
- エラー修正後、再度インポート実行

#### 典型的なエラーと対処法

**1. CSV構文エラー**
```
CSV構文エラー
Line 5: Column count mismatch. Expected 15, got 14
```
→ **対処**: 該当行の列数を確認。カンマの数、ダブルクォートの閉じ忘れをチェック

**2. 必須項目欠如**
```
Row 3: challenge_math_003

エラー内容:
- questionTextが空です
- correctAnswersが空です
```
→ **対処**: 必須項目（challengeId, challengeName, questionText, correctAnswers）を入力

**3. Enum値不正**
```
Row 7: challenge_quiz_001

エラー内容:
- 不正なdifficulty値: Mediam
```
→ **対処**: Enum値のスペルミスを修正（`Mediam` → `Medium` ※ただし正しくは `Normal`）

**4. JSON解析エラー**
```
Row 10: challenge_logic_001

エラー内容:
- rewardItemsのJSON解析エラー: Invalid JSON
```
→ **対処**: JSON構文を確認。ダブルクォートのエスケープ（`""`）、カンマ、括弧の対応をチェック

#### デバッグのコツ

1. **小規模テスト**: まずサンプルCSVでインポート成功を確認
2. **段階的追加**: 10問ずつなど、小分けにしてインポート
3. **Excel活用**: Excelで編集すると、ダブルクォートのエスケープが自動化されて便利
4. **バックアップ**: 大量インポート前にプロジェクトをバックアップ

### パフォーマンス

| 問題数 | 所要時間（目安） | メモリ |
|-------|---------------|--------|
| 10問 | 1-3秒 | 低 |
| 50問 | 5-10秒 | 低 |
| 100問 | 10-20秒 | 中 |
| 200問 | 20-40秒 | 中 |
| 500問 | 50-100秒 | 高 |

**最適化ポイント:**
- 100問ごとに自動保存（`AssetDatabase.SaveAssets()`）
- プログレスバーでキャンセル可能

### よくある質問

**Q: 既存の問題データを上書きしたくない**  
A: 同じ `challengeId` のアセットが存在する場合、確認ダイアログが表示されます。「キャンセル」を選択すればインポート中断されます。

**Q: CSVファイルの文字コードは？**  
A: UTF-8推奨。BOM付きUTF-8も自動認識されます。Shift_JISなどは文字化けの可能性があります。

**Q: rewardItemsを空にしたい**  
A: `[]` または空文字列を指定してください。

**Q: correctAnswersに改行を含めたい**  
A: CSVの制約上、改行は扱えません。代替案として `\n` を含む文字列を設定し、ゲーム側で置換してください。

**Q: 大量の問題を一度にインポートしても大丈夫？**  
A: 500問程度までは問題ありませんが、エラー発生時のデバッグを考慮し、100問ずつなど分割インポートを推奨します。

---

## GameDatabase統合（将来の拡張用）

現在、ChallengeMasterDatabaseは独立して動作します。将来的にGameDatabaseと統合する場合は、以下を参考にしてください。

### MasterDatabaseへの統合

`MasterDatabase.cs` に以下を追加：

```csharp
[Header("問題マスターデータ")]
[SerializeField] private List<ChallengeData> allChallenges = new List<ChallengeData>();

/// <summary>
/// すべての問題データを取得
/// </summary>
public List<ChallengeData> GetAllChallengeData()
{
    return new List<ChallengeData>(allChallenges);
}

/// <summary>
/// IDで問題データを取得
/// </summary>
public ChallengeData GetChallengeData(string challengeId)
{
    return allChallenges.Find(c => c.challengeId == challengeId);
}

/// <summary>
/// 難易度別にランダムで問題を取得
/// </summary>
public ChallengeData GetRandomChallengeByDifficulty(ChallengeDifficulty difficulty)
{
    return ChallengeDatabaseHelper.GetRandomChallengeByDifficulty(allChallenges, difficulty);
}

/// <summary>
/// 問題データを追加（エディタ用）
/// </summary>
public void AddChallengeData(ChallengeData challengeData)
{
    if (!allChallenges.Contains(challengeData))
    {
        allChallenges.Add(challengeData);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
```

### GameDatabaseへの統合

`GameDatabase.cs` に以下を追加：

```csharp
[Header("問題データ")]
[SerializeField] private List<Challenge> challenges = new List<Challenge>();

/// <summary>
/// すべての問題を取得
/// </summary>
public List<Challenge> GetAllChallenges()
{
    return new List<Challenge>(challenges);
}

/// <summary>
/// 問題を追加
/// </summary>
public void AddChallenge(Challenge challenge)
{
    Challenge existing = ChallengeDatabaseHelper.FindChallengeById(challenges, challenge.challengeId);
    if (existing == null)
    {
        challenges.Add(challenge);
        Debug.Log($"問題「{challenge.challengeName}」を追加しました。");
    }
}

/// <summary>
/// 問題を解放
/// </summary>
public bool UnlockChallenge(string challengeId)
{
    Challenge challenge = ChallengeDatabaseHelper.FindChallengeById(challenges, challengeId);
    if (challenge != null)
    {
        challenge.Unlock();
        return true;
    }
    
    // MasterDatabaseから作成
    ChallengeData challengeData = MasterDatabase.Instance.GetChallengeData(challengeId);
    if (challengeData != null)
    {
        Challenge newChallenge = challengeData.CreateChallenge();
        newChallenge.Unlock();
        AddChallenge(newChallenge);
        return true;
    }
    
    Debug.LogWarning($"問題「{challengeId}」が見つかりません。");
    return false;
}

/// <summary>
/// 回答を送信してクリア判定
/// </summary>
public bool SubmitChallengeAnswer(string challengeId, string userAnswer)
{
    Challenge challenge = ChallengeDatabaseHelper.FindChallengeById(challenges, challengeId);
    if (challenge == null || challenge.status != ChallengeStatus.Unlocked)
    {
        return false;
    }
    
    challenge.IncrementAttempt();
    bool isCorrect = challenge.CheckAnswer(userAnswer);
    
    if (isCorrect)
    {
        challenge.RecordCompletion();
        
        // 報酬付与
        ChallengeData challengeData = MasterDatabase.Instance.GetChallengeData(challengeId);
        if (challengeData != null)
        {
            ChallengeDatabaseHelper.GiveRewards(
                challengeData.rewardTable,
                onItemReward: (itemId, quantity) =>
                {
                    ItemData itemData = MasterDatabase.Instance.GetItemData(itemId);
                    if (itemData != null)
                    {
                        AddItem(itemData.itemId, itemData.itemName, itemData.description,
                               quantity, itemData.type);
                    }
                },
                onMoneyReward: (amount) =>
                {
                    AddMoney(amount);
                }
            );
        }
    }
    
    return isCorrect;
}

/// <summary>
/// クリア済み問題数を取得
/// </summary>
public int GetCompletedChallengeCount()
{
    return ChallengeDatabaseHelper.GetCompletedChallengeCount(challenges);
}
```

### 統合後の使用例

```csharp
// 難易度を選択してランダムに問題を取得
ChallengeData challengeData = MasterDatabase.Instance.GetRandomChallengeByDifficulty(
    ChallengeDifficulty.Normal
);

// GameDatabaseに追加して解放
GameDatabase.Instance.UnlockChallenge(challengeData.challengeId);

// 回答を送信
bool isCorrect = GameDatabase.Instance.SubmitChallengeAnswer(
    challengeData.challengeId, 
    userInputField.text
);

if (isCorrect)
{
    Debug.Log("正解！報酬を獲得しました！");
}
```

---

## トラブルシューティング

### Q. 問題が選択されない

**A.** 以下を確認してください：
- `testChallenges` リストに問題データが追加されているか
- 指定した難易度の問題が存在するか
- 問題データがnullではないか

### Q. 正解なのに不正解になる

**A.** 以下を確認してください：
- `Correct Answers` に正しい値が入っているか
- **数値問題**: `Numeric Tolerance` の設定が適切か
- **テキスト問題**: `Case Sensitive` の設定が適切か
- 空白文字が含まれていないか（自動でTrimされます）

### Q. 報酬がドロップしない

**A.** これは正常な動作です。`Drop Rate` が1.0未満の場合、確率で抽選されます。何度か試してください。テスト時は `Drop Rate` を1.0に設定すると確実にドロップします。

### Q. MasterDatabaseに統合できない

**A.** 現在、このシステムは独立したモジュールとして動作します。統合は上記の手順を参考にしてください。共同作業者と調整の上、慎重に統合してください。

### Q. コンパイルエラーが出る

**A.** すべてのファイルが `Assets/Scripts/ChallengeDB/` に配置されているか確認してください。また、Unity 2021.3以降を推奨します。

---

## API リファレンス

### ChallengeData（ScriptableObject）

| メンバー | 型 | 説明 |
|---------|---|------|
| `challengeId` | `string` | 問題の一意なID |
| `challengeName` | `string` | 問題の表示名 |
| `difficulty` | `ChallengeDifficulty` | 難易度 |
| `questionText` | `string` | 問題文 |
| `correctAnswers` | `string[]` | 正解パターン |
| `rewardTable` | `RewardTable` | 報酬テーブル |
| `CreateChallenge()` | `Challenge` | ランタイムインスタンス生成 |
| `Validate(out string)` | `bool` | データ妥当性チェック |

### Challenge（Serializable）

| メンバー | 型 | 説明 |
|---------|---|------|
| `status` | `ChallengeStatus` | 現在の状態 |
| `attemptCount` | `int` | 挑戦回数 |
| `completedCount` | `int` | クリア回数 |
| `CheckAnswer(string)` | `bool` | 回答チェック |
| `IncrementAttempt()` | `void` | 挑戦回数を増やす |
| `RecordCompletion()` | `void` | クリア記録 |
| `Unlock()` | `void` | 問題を解放 |

### ChallengeMasterDatabase（ScriptableObject - シングルトン）

| メンバー | 型 | 説明 |
|---------|---|------|
| `Instance` | `static ChallengeMasterDatabase` | シングルトンインスタンス（Resources.Load） |
| `GetAllChallengeData()` | `List<ChallengeData>` | すべての問題データを取得 |
| `GetChallengeData(string)` | `ChallengeData` | IDで問題データを取得 |
| `GetChallengeDataByDifficulty()` | `List<ChallengeData>` | 難易度別に問題を取得 |
| `GetRandomChallengeByDifficulty()` | `ChallengeData` | 難易度別ランダム選択 |
| `AddChallengeData()` | `void` | 問題データを登録（エディタ用） |
| `RemoveChallengeData()` | `bool` | 問題データを削除（エディタ用） |
| `GetChallengeCount()` | `int` | 総問題数を取得 |
| `GetChallengeCountByDifficulty()` | `int` | 難易度別の問題数を取得 |
| `GetStatistics()` | `string` | 統計情報を文字列で取得 |

**使用例:**
```csharp
// シングルトン経由でアクセス
ChallengeMasterDatabase.Instance.GetRandomChallengeByDifficulty(ChallengeDifficulty.Normal);
```

### ChallengeDatabaseHelper（静的）

| メソッド | 説明 |
|---------|------|
| `GetRandomChallengeByDifficulty()` | 難易度別ランダム選択 |
| `FilterByDifficulty()` | 難易度でフィルタ |
| `GiveRewards()` | 報酬付与 |
| `GetCompletedChallengeCount()` | クリア数取得 |
| `GetStatistics()` | 統計情報取得 |

---

## 更新履歴

### v1.1.0 (2026/02/19)
- **CSV一括インポート機能追加**
  - CSVSimpleParser: RFC 4180準拠のCSVパーサー
  - ChallengeCSVParser: CSV行からChallengeData生成
  - ChallengeCSVImporter: EditorWindowによる一括インポートUI
  - JSON形式でのRewardItems記述対応
  - エラー時即座中断方式の採用
  - サンプルCSVファイル（10問）を添付
  - 詳細なエラーハンドリングとデバッグ機能
- README更新: CSV形式仕様、インポート手順、トラブルシューティングを追加

### v1.0.0 (2026/02/15)
- 初回リリース
- 入力式問題システムの基本実装
- 5段階難易度システム
- 確率ベース報酬システム
- ランダム出題機能
- 複数正解パターン対応

---

## ライセンス・連絡先

このシステムはTest_Projectの一部として実装されています。

**作成日**: 2026年2月15日  
**実装者**: GitHub Copilot (Claude Sonnet 4.5)
