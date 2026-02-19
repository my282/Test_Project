using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Challenge Database用のヘルパーメソッド集
/// 後でGameDatabaseに統合しやすいよう、静的メソッドとして実装
/// </summary>
public static class ChallengeDatabaseHelper
{
    /// <summary>
    /// 指定した難易度の問題データをフィルタリング
    /// </summary>
    /// <param name="allChallenges">すべての問題データ</param>
    /// <param name="difficulty">フィルタする難易度</param>
    /// <returns>フィルタされた問題データリスト</returns>
    public static List<ChallengeData> FilterByDifficulty(
        List<ChallengeData> allChallenges, 
        ChallengeDifficulty difficulty)
    {
        if (allChallenges == null)
        {
            return new List<ChallengeData>();
        }
        
        return allChallenges.Where(c => c.difficulty == difficulty).ToList();
    }
    
    /// <summary>
    /// 指定した難易度からランダムに問題データを1つ選択
    /// </summary>
    /// <param name="allChallenges">すべての問題データ</param>
    /// <param name="difficulty">選択する難易度</param>
    /// <returns>ランダムに選ばれた問題データ（該当なしの場合null）</returns>
    public static ChallengeData GetRandomChallengeByDifficulty(
        List<ChallengeData> allChallenges, 
        ChallengeDifficulty difficulty)
    {
        List<ChallengeData> filtered = FilterByDifficulty(allChallenges, difficulty);
        
        if (filtered.Count == 0)
        {
            Debug.LogWarning($"難易度 {difficulty} の問題が見つかりませんでした。");
            return null;
        }
        
        int randomIndex = UnityEngine.Random.Range(0, filtered.Count);
        return filtered[randomIndex];
    }
    
    /// <summary>
    /// 問題の種類でフィルタリング
    /// </summary>
    /// <param name="allChallenges">すべての問題データ</param>
    /// <param name="type">フィルタする種類</param>
    /// <returns>フィルタされた問題データリスト</returns>
    public static List<ChallengeData> FilterByType(
        List<ChallengeData> allChallenges, 
        ChallengeType type)
    {
        if (allChallenges == null)
        {
            return new List<ChallengeData>();
        }
        
        return allChallenges.Where(c => c.type == type).ToList();
    }
    
    /// <summary>
    /// 難易度と種類の両方でフィルタリング
    /// </summary>
    public static List<ChallengeData> FilterByDifficultyAndType(
        List<ChallengeData> allChallenges,
        ChallengeDifficulty difficulty,
        ChallengeType type)
    {
        if (allChallenges == null)
        {
            return new List<ChallengeData>();
        }
        
        return allChallenges
            .Where(c => c.difficulty == difficulty && c.type == type)
            .ToList();
    }
    
    /// <summary>
    /// 報酬を付与（コールバック方式）
    /// 後でGameDatabase.AddItem()、AddMoney()と連携しやすい形
    /// </summary>
    /// <param name="rewardTable">報酬テーブル</param>
    /// <param name="onItemReward">アイテム獲得時のコールバック(itemId, quantity)</param>
    /// <param name="onMoneyReward">お金獲得時のコールバック(amount)</param>
    public static void GiveRewards(
        RewardTable rewardTable,
        Action<string, int> onItemReward,
        Action<int> onMoneyReward)
    {
        if (rewardTable == null)
        {
            Debug.LogWarning("RewardTableがnullです。");
            return;
        }
        
        // アイテムの報酬抽選
        var rewards = rewardTable.GetRandomRewards();
        foreach (var (itemId, quantity) in rewards)
        {
            onItemReward?.Invoke(itemId, quantity);
            Debug.Log($"アイテム「{itemId}」を {quantity}個 獲得しました！");
        }
        
        if (rewards.Count == 0)
        {
            Debug.Log("今回は報酬がありませんでした。");
        }
    }
    
    /// <summary>
    /// Challengeリストから特定IDの問題を検索
    /// </summary>
    public static Challenge FindChallengeById(List<Challenge> challenges, string challengeId)
    {
        if (challenges == null || string.IsNullOrEmpty(challengeId))
        {
            return null;
        }
        
        return challenges.FirstOrDefault(c => c.challengeId == challengeId);
    }
    
    /// <summary>
    /// 指定した状態の問題を取得
    /// </summary>
    public static List<Challenge> GetChallengesByStatus(
        List<Challenge> challenges, 
        ChallengeStatus status)
    {
        if (challenges == null)
        {
            return new List<Challenge>();
        }
        
        return challenges.Where(c => c.status == status).ToList();
    }
    
    /// <summary>
    /// クリア済みの問題数を取得
    /// </summary>
    public static int GetCompletedChallengeCount(List<Challenge> challenges)
    {
        if (challenges == null)
        {
            return 0;
        }
        
        return challenges.Count(c => c.status == ChallengeStatus.Completed);
    }
    
    /// <summary>
    /// 難易度別のクリア済み問題数を取得
    /// </summary>
    public static int GetCompletedChallengeCountByDifficulty(
        List<Challenge> challenges, 
        ChallengeDifficulty difficulty)
    {
        if (challenges == null)
        {
            return 0;
        }
        
        return challenges.Count(c => 
            c.status == ChallengeStatus.Completed && c.difficulty == difficulty);
    }
    
    /// <summary>
    /// 統計情報を取得
    /// </summary>
    public static string GetStatistics(List<Challenge> challenges)
    {
        if (challenges == null || challenges.Count == 0)
        {
            return "統計データがありません。";
        }
        
        int total = challenges.Count;
        int completed = GetCompletedChallengeCount(challenges);
        int unlocked = challenges.Count(c => c.status == ChallengeStatus.Unlocked);
        int locked = challenges.Count(c => c.status == ChallengeStatus.Locked);
        int totalAttempts = challenges.Sum(c => c.attemptCount);
        
        return $"=== Challenge統計 ===\n" +
               $"総問題数: {total}\n" +
               $"クリア済み: {completed}\n" +
               $"解放済み: {unlocked}\n" +
               $"ロック中: {locked}\n" +
               $"総挑戦回数: {totalAttempts}";
    }
}
