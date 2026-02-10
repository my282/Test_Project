using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Facilityの生成タイプ
/// </summary>
public enum ProductionType
{
    None,           // 生成なし
    Money,          // お金のみ
    Item,           // アイテムのみ
    Both            // お金とアイテム両方
}

/// <summary>
/// Facilityの自動生成設定
/// </summary>
[Serializable]
public class ProductionConfig
{
    [Header("生成タイプ")]
    [Tooltip("何を生成するか")]
    public ProductionType productionType = ProductionType.None;

    [Header("お金の生成設定")]
    [Tooltip("生成する金額")]
    public int moneyAmount = 0;

    [Header("アイテムの生成設定")]
    [Tooltip("生成するアイテムのリスト")]
    public List<ItemProduction> itemProductions = new List<ItemProduction>();

    [Header("生成間隔設定")]
    [Tooltip("生成間隔（秒）")]
    public float productionInterval = 10f;

    [Tooltip("自動生成を有効化")]
    public bool enableAutoProduction = true;

    // TODO: タイマー機能実装後の統合ポイント
    // TimerManagerと統合する際は以下の実装を追加：
    // 1. TimerManager.Instance.CheckInterval() を使用して時間チェック
    // 2. TimerManager.OnTimerFinished イベントでゲーム終了時の処理
    // 3. TimerManager.OnTimerPaused で一時停止時の生成停止
    // 
    // 実装例:
    // if (TimerManager.Instance.CheckInterval(productionInterval, ref lastProductionTime))
    // {
    //     ProduceResources();
    // }

    /// <summary>
    /// お金を生成するか
    /// </summary>
    public bool ProducesMoney()
    {
        return productionType == ProductionType.Money || productionType == ProductionType.Both;
    }

    /// <summary>
    /// アイテムを生成するか
    /// </summary>
    public bool ProducesItems()
    {
        return productionType == ProductionType.Item || productionType == ProductionType.Both;
    }

    /// <summary>
    /// 設定が有効か
    /// </summary>
    public bool IsValid()
    {
        if (!enableAutoProduction || productionType == ProductionType.None)
            return false;

        if (ProducesMoney() && moneyAmount <= 0)
            return false;

        if (ProducesItems() && (itemProductions == null || itemProductions.Count == 0))
            return false;

        return true;
    }
}

/// <summary>
/// アイテム生成の詳細設定
/// </summary>
[Serializable]
public class ItemProduction
{
    [Tooltip("生成するアイテムのID")]
    public string itemId;

    [Tooltip("ItemDataへの参照（オプション）")]
    public ItemData itemData;

    [Tooltip("生成する数量")]
    public int quantity = 1;

    /// <summary>
    /// アイテムIDを取得（itemDataが設定されていればそちらを優先）
    /// </summary>
    public string GetItemId()
    {
        if (itemData != null)
            return itemData.itemId;
        return itemId;
    }
}

/// <summary>
/// Facilityの生成状態を管理
/// </summary>
[Serializable]
public class ProductionState
{
    [Tooltip("最後に生成した時刻（秒）")]
    public float lastProductionTime;

    [Tooltip("累計生成回数")]
    public int totalProductionCount;

    [Tooltip("累計お金生成量")]
    public long totalMoneyProduced;

    [Tooltip("累計アイテム生成量（アイテムID別）")]
    public Dictionary<string, int> totalItemsProduced = new Dictionary<string, int>();

    [Tooltip("生成が一時停止中")]
    public bool isPaused;

    /// <summary>
    /// 状態をリセット
    /// </summary>
    public void Reset()
    {
        lastProductionTime = 0f;
        totalProductionCount = 0;
        totalMoneyProduced = 0;
        totalItemsProduced.Clear();
        isPaused = false;
    }

    /// <summary>
    /// 生成記録を追加
    /// </summary>
    public void RecordProduction(int money, Dictionary<string, int> items)
    {
        totalProductionCount++;
        totalMoneyProduced += money;

        if (items != null)
        {
            foreach (var item in items)
            {
                if (totalItemsProduced.ContainsKey(item.Key))
                {
                    totalItemsProduced[item.Key] += item.Value;
                }
                else
                {
                    totalItemsProduced[item.Key] = item.Value;
                }
            }
        }
    }
}
