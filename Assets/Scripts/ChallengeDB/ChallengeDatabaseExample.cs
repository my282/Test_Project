using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Challenge Database システムの使用例とデモ
/// DatabaseExample.csと同様のパターンで実装
/// </summary>
public class ChallengeDatabaseExample : MonoBehaviour
{
    [Header("問題データ設定")]
    [Tooltip("テスト用の問題データリスト")]
    public List<ChallengeData> testChallenges = new List<ChallengeData>();
    
    [Header("現在のテスト設定")]
    [Tooltip("テストする難易度")]
    public ChallengeDifficulty testDifficulty = ChallengeDifficulty.Normal;
    
    [Tooltip("テスト回答（インスペクターから入力）")]
    public string testAnswer = "";
    
    [Header("ランタイムデータ")]
    [Tooltip("現在選択されている問題")]
    [SerializeField] private Challenge currentChallenge;
    
    [Tooltip("プレイヤーの問題リスト")]
    [SerializeField] private List<Challenge> playerChallenges = new List<Challenge>();
    
    private void Start()
    {
        Debug.Log("=== Challenge Database Example 開始 ===");
        Debug.Log("コンテキストメニュー（右クリック）からテストを実行できます。");
        Debug.Log("または、Inspectorでパラメータを設定してテストしてください。");
    }
    
    /// <summary>
    /// ランダムに問題を選択
    /// </summary>
    [ContextMenu("1. ランダムに問題を選択")]
    public void TestSelectRandomChallenge()
    {
        Debug.Log("=== ランダム問題選択テスト ===");
        
        if (testChallenges.Count == 0)
        {
            Debug.LogWarning("testChallengesリストが空です。インスペクターで設定してください。");
            return;
        }
        
        // ランダムに選択
        ChallengeData selectedData = ChallengeDatabaseHelper.GetRandomChallengeByDifficulty(
            testChallenges, 
            testDifficulty
        );
        
        if (selectedData != null)
        {
            // Challengeインスタンスを生成
            currentChallenge = selectedData.CreateChallenge();
            currentChallenge.Unlock(); // テストのため自動解放
            
            Debug.Log($"選択された問題: {currentChallenge.challengeName}");
            Debug.Log($"難易度: {currentChallenge.difficulty}");
            Debug.Log($"問題文: {currentChallenge.questionText}");
            Debug.Log($"正解パターン数: {currentChallenge.correctAnswers.Length}");
            
            // プレイヤーリストに追加（重複チェック）
            Challenge existing = ChallengeDatabaseHelper.FindChallengeById(
                playerChallenges, 
                currentChallenge.challengeId
            );
            
            if (existing == null)
            {
                playerChallenges.Add(currentChallenge);
                Debug.Log("問題をプレイヤーリストに追加しました。");
            }
            else
            {
                Debug.Log("この問題は既にプレイヤーリストにあります。");
            }
        }
        else
        {
            Debug.LogWarning($"難易度 {testDifficulty} の問題が見つかりませんでした。");
        }
    }
    
    /// <summary>
    /// 回答をチェック
    /// </summary>
    [ContextMenu("2. 回答をチェック")]
    public void TestCheckAnswer()
    {
        Debug.Log("=== 回答チェックテスト ===");
        
        if (currentChallenge == null)
        {
            Debug.LogWarning("問題が選択されていません。先に「1. ランダムに問題を選択」を実行してください。");
            return;
        }
        
        if (string.IsNullOrEmpty(testAnswer))
        {
            Debug.LogWarning("testAnswerが空です。インスペクターで回答を入力してください。");
            return;
        }
        
        Debug.Log($"問題: {currentChallenge.questionText}");
        Debug.Log($"あなたの回答: {testAnswer}");
        
        // 挑戦回数を増やす
        currentChallenge.IncrementAttempt();
        
        // 回答チェック
        bool isCorrect = currentChallenge.CheckAnswer(testAnswer);
        
        if (isCorrect)
        {
            Debug.Log("✓ 正解です！");
            currentChallenge.RecordCompletion();
            
            // 対応するChallengeDataを探して報酬を付与
            ChallengeData data = testChallenges.Find(c => c.challengeId == currentChallenge.challengeId);
            if (data != null)
            {
                TestGiveReward(data.rewardTable);
            }
        }
        else
        {
            Debug.Log("✗ 不正解です。もう一度試してください。");
            Debug.Log($"挑戦回数: {currentChallenge.attemptCount}回");
        }
    }
    
    /// <summary>
    /// 報酬を付与（テスト）
    /// </summary>
    [ContextMenu("3. 報酬抽選テスト")]
    public void TestGiveRewardCurrent()
    {
        if (currentChallenge == null)
        {
            Debug.LogWarning("問題が選択されていません。");
            return;
        }
        
        ChallengeData data = testChallenges.Find(c => c.challengeId == currentChallenge.challengeId);
        if (data != null)
        {
            TestGiveReward(data.rewardTable);
        }
        else
        {
            Debug.LogWarning("ChallengeDataが見つかりません。");
        }
    }
    
    /// <summary>
    /// 報酬付与のヘルパーメソッド
    /// </summary>
    private void TestGiveReward(RewardTable rewardTable)
    {
        Debug.Log("=== 報酬抽選 ===");
        
        ChallengeDatabaseHelper.GiveRewards(
            rewardTable,
            onItemReward: (itemId, quantity) =>
            {
                // 実際のゲームではGameDatabase.AddItem()を呼ぶ
                Debug.Log($"[獲得] アイテム: {itemId} x {quantity}");
            },
            onMoneyReward: (amount) =>
            {
                // 実際のゲームではGameDatabase.AddMoney()を呼ぶ
                Debug.Log($"[獲得] お金: {amount}");
            }
        );
    }
    
    /// <summary>
    /// 統計情報を表示
    /// </summary>
    [ContextMenu("4. 統計情報を表示")]
    public void TestShowStatistics()
    {
        Debug.Log("=== 統計情報 ===");
        string stats = ChallengeDatabaseHelper.GetStatistics(playerChallenges);
        Debug.Log(stats);
        
        if (currentChallenge != null)
        {
            Debug.Log("\n=== 現在の問題 ===");
            Debug.Log(currentChallenge.GetDebugInfo());
        }
    }
    
    /// <summary>
    /// すべてのテストをリセット
    /// </summary>
    [ContextMenu("5. テストをリセット")]
    public void TestReset()
    {
        currentChallenge = null;
        playerChallenges.Clear();
        testAnswer = "";
        Debug.Log("テストをリセットしました。");
    }
    
    /// <summary>
    /// 難易度別フィルタテスト
    /// </summary>
    [ContextMenu("6. 難易度別フィルタテスト")]
    public void TestFilterByDifficulty()
    {
        Debug.Log($"=== 難易度フィルタテスト（{testDifficulty}） ===");
        
        var filtered = ChallengeDatabaseHelper.FilterByDifficulty(testChallenges, testDifficulty);
        Debug.Log($"該当する問題数: {filtered.Count}");
        
        foreach (var challenge in filtered)
        {
            Debug.Log($"- {challenge.challengeName} ({challenge.difficulty})");
        }
    }
    
    /// <summary>
    /// バリデーションテスト
    /// </summary>
    [ContextMenu("7. バリデーションテスト")]
    public void TestValidation()
    {
        Debug.Log("=== バリデーションテスト ===");
        
        foreach (var challengeData in testChallenges)
        {
            if (challengeData.Validate(out string errorMessage))
            {
                Debug.Log($"✓ {challengeData.challengeName}: 有効");
            }
            else
            {
                Debug.LogError($"✗ {challengeData.challengeName}: {errorMessage}");
            }
        }
    }
}
