# Challenge Database システム

**入力式問題システムの完全実装**

## 📋 目次

- [概要](#概要)
- [システム構成](#システム構成)
- [クイックスタート](#クイックスタート)
- [詳細ガイド](#詳細ガイド)
- [MasterDatabase/GameDatabase統合](#masterdatabasegamedatabase統合)
- [トラブルシューティング](#トラブルシューティング)

---

## 概要

Challenge Databaseシステムは、入力式問題を管理・実行するためのフレームワークです。以下の機能を提供します：

### 主な機能

✅ **入力式問題対応** - ユーザーがテキストや数値を入力して回答  
✅ **5段階難易度** - VeryEasy～VeryHardの難易度設定  
✅ **ランダム出題** - 難易度別にランダムで問題を選択  
✅ **確率報酬システム** - ガチャ形式の報酬抽選  
✅ **複数正解パターン** - 1つの問題に複数の正解を設定可能  
✅ **数値許容範囲** - 小数点の誤差を考慮した判定  
✅ **大小文字設定** - テキスト回答の大小文字区別/無視  
✅ **進行状態管理** - 挑戦回数、クリア回数、最終プレイ日時を記録

### アーキテクチャ

既存のItem/Facilityシステムと同じ**二層構造**を採用：

```
┌─────────────────────────────────┐
│  ChallengeData (ScriptableObject) │  ← マスターデータ（静的）
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

| ファイル名 | 役割 | 説明 |
|-----------|------|------|
| `ChallengeEnums.cs` | Enum定義 | 難易度、種類、状態などの列挙型 |
| `RewardSystem.cs` | 報酬システム | 確率ベースの報酬抽選ロジック |
| `ChallengeData.cs` | マスターデータ | ScriptableObject、問題の定義 |
| `Challenge.cs` | ランタイムデータ | プレイヤーの進行状況を管理 |
| `ChallengeDatabaseHelper.cs` | ヘルパー | ユーティリティ関数集 |
| `ChallengeDatabaseExample.cs` | デモ | 使用例とテストスクリプト |

### Enum定義

#### ChallengeDifficulty（難易度）
```csharp
public enum ChallengeDifficulty
{
    VeryEasy,   // 非常に簡単
    Easy,       // 簡単
    Normal,     // 普通
    Hard,       // 難しい
    VeryHard    // 非常に難しい
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

### 3. テスト実行

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

---

## MasterDatabase/GameDatabase統合

現在、このシステムは**独立したモジュール**として実装されています。以下の手順で既存のDatabase系に統合できます。

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
