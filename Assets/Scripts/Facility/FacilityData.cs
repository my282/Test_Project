using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 設備のマスターデータ（ScriptableObject）
/// </summary>
[CreateAssetMenu(fileName = "NewFacility", menuName = "Game/Facility Data")]
public class FacilityData : ScriptableObject
{
    [Header("基本情報")]
    public string facilityId;       // 設備ID
    public string facilityName;     // 設備名
    [TextArea(3, 5)]
    public string description;      // 説明
    public Sprite icon;             // アイコン画像
    public FacilityType type;       // 設備タイプ

    [Header("設定")]
    public int maxLevel = 10;       // 最大レベル
    
    [Header("解放コスト")]
    public int unlockMoneyCost = 0;             // 解放に必要なお金
    public List<ItemCost> unlockItemCosts = new List<ItemCost>();  // 解放に必要なアイテム
    
    [Header("アップグレードコスト")]
    public int upgradeMoneyCost = 0;            // アップグレードに必要なお金
    public List<ItemCost> upgradeItemCosts = new List<ItemCost>(); // アップグレードに必要なアイテム

    [Header("自動生成設定")]
    [Tooltip("この設備の自動生成設定")]
    public ProductionConfig productionConfig = new ProductionConfig();

    [Header("レベルによる生成量増加")]
    [Tooltip("レベルが1上がるごとの生成量増加倍率（例: 1.0ならLv1=1倍, Lv2=2倍, Lv3=3倍）")]
    public float productionAmountIncreasePerLevel = 1.0f;

    /// <summary>
    /// Facilityインスタンスを生成
    /// </summary>
    public Facility CreateFacility(int level = 1, bool isUnlocked = false)
    {
        return new Facility(facilityId, facilityName, description, level, isUnlocked, type, productionConfig);
    }
}
