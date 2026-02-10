using System;
using UnityEngine;

/// <summary>
/// 設備のデータ構造
/// </summary>
[Serializable]
public class Facility
{
    public string facilityId;       // 設備ID
    public string facilityName;     // 設備名
    public string description;      // 説明
    public int level;               // レベル
    public bool isUnlocked;         // 解放済みかどうか
    public FacilityType type;       // 設備タイプ

    [Header("自動生成設定")]
    public ProductionConfig productionConfig;   // 生成設定
    public ProductionState productionState;     // 生成状態

    public Facility(string id, string name, string desc, int lvl, bool unlocked, FacilityType facilityType, ProductionConfig prodConfig = null)
    {
        facilityId = id;
        facilityName = name;
        description = desc;
        level = lvl;
        isUnlocked = unlocked;
        type = facilityType;
        
        // 生成設定をコピー（nullの場合は新規作成）
        if (prodConfig != null)
        {
            productionConfig = prodConfig;
        }
        else
        {
            productionConfig = new ProductionConfig();
        }
        
        // 生成状態を初期化
        productionState = new ProductionState();
    }

    /// <summary>
    /// 生成設定を更新
    /// </summary>
    public void UpdateProductionConfig(ProductionConfig newConfig)
    {
        productionConfig = newConfig;
    }

    /// <summary>
    /// 生成タイプを設定
    /// </summary>
    public void SetProductionType(ProductionType type)
    {
        productionConfig.productionType = type;
    }

    /// <summary>
    /// 生成金額を設定
    /// </summary>
    public void SetMoneyAmount(int amount)
    {
        productionConfig.moneyAmount = amount;
    }

    /// <summary>
    /// 生成間隔を設定
    /// </summary>
    public void SetProductionInterval(float interval)
    {
        productionConfig.productionInterval = interval;
    }

    /// <summary>
    /// アイテム生成を追加
    /// </summary>
    public void AddItemProduction(string itemId, int quantity)
    {
        if (productionConfig.itemProductions == null)
        {
            productionConfig.itemProductions = new System.Collections.Generic.List<ItemProduction>();
        }

        productionConfig.itemProductions.Add(new ItemProduction
        {
            itemId = itemId,
            quantity = quantity
        });
    }

    /// <summary>
    /// アイテム生成を追加（ItemData使用）
    /// </summary>
    public void AddItemProduction(ItemData itemData, int quantity)
    {
        if (productionConfig.itemProductions == null)
        {
            productionConfig.itemProductions = new System.Collections.Generic.List<ItemProduction>();
        }

        productionConfig.itemProductions.Add(new ItemProduction
        {
            itemData = itemData,
            quantity = quantity
        });
    }

    /// <summary>
    /// 自動生成を有効/無効化
    /// </summary>
    public void SetAutoProductionEnabled(bool enabled)
    {
        productionConfig.enableAutoProduction = enabled;
    }

    /// <summary>
    /// 生成状態をリセット
    /// </summary>
    public void ResetProductionState()
    {
        productionState.Reset();
    }
}

/// <summary>
/// 設備の種類
/// </summary>
public enum FacilityType
{
    Production,     // 生産施設
    Storage,        // 保管施設
    Defense,        // 防衛施設
    Research,       // 研究施設
    Other           // その他
}
