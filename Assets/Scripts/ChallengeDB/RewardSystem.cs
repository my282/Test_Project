using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 報酬アイテムの定義（確率ベース）
/// </summary>
[Serializable]
public class RewardItem
{
    [Header("アイテム情報")]
    [Tooltip("報酬アイテムのID")]
    public string itemId;
    
    [Header("数量設定")]
    [Tooltip("最小個数")]
    [Min(1)]
    public int minQuantity = 1;
    
    [Tooltip("最大個数")]
    [Min(1)]
    public int maxQuantity = 1;
    
    [Header("ドロップ確率")]
    [Tooltip("ドロップ率（0.0～1.0）")]
    [Range(0f, 1f)]
    public float dropRate = 0.5f;
    
    /// <summary>
    /// ドロップ判定を行い、成功した場合はランダムな数量を返す
    /// </summary>
    /// <returns>ドロップした場合は数量、しなかった場合は0</returns>
    public int RollDrop()
    {
        float roll = UnityEngine.Random.value;
        if (roll <= dropRate)
        {
            return UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
        }
        return 0;
    }
}

/// <summary>
/// 報酬テーブル（複数のRewardItemを管理）
/// </summary>
[Serializable]
public class RewardTable
{
    [Header("アイテム報酬")]
    [Tooltip("確率でドロップするアイテムのリスト")]
    public List<RewardItem> rewardItems = new List<RewardItem>();
    
    /// <summary>
    /// 報酬抽選を実行し、獲得したアイテムのリストを返す
    /// </summary>
    /// <returns>アイテムIDと数量のペアのリスト</returns>
    public List<(string itemId, int quantity)> GetRandomRewards()
    {
        List<(string, int)> rewards = new List<(string, int)>();
        
        foreach (var rewardItem in rewardItems)
        {
            int quantity = rewardItem.RollDrop();
            if (quantity > 0)
            {
                rewards.Add((rewardItem.itemId, quantity));
            }
        }
        
        return rewards;
    }
    
    /// <summary>
    /// 全アイテムの最大期待値を計算（デバッグ用）
    /// </summary>
    public float CalculateExpectedValue()
    {
        float total = 0f;
        foreach (var rewardItem in rewardItems)
        {
            float avgQuantity = (rewardItem.minQuantity + rewardItem.maxQuantity) / 2f;
            total += avgQuantity * rewardItem.dropRate;
        }
        return total;
    }
    
    /// <summary>
    /// 全ドロップ率の合計を取得（エディタ表示用）
    /// </summary>
    public float GetTotalDropRate()
    {
        float total = 0f;
        foreach (var rewardItem in rewardItems)
        {
            total += rewardItem.dropRate;
        }
        return total;
    }
}
