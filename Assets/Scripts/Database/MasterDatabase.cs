using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ゲーム内のすべてのアイテムと設備のマスターデータを管理
/// </summary>
[CreateAssetMenu(fileName = "MasterDatabase", menuName = "Game/Master Database")]
public class MasterDatabase : ScriptableObject
{
    private static MasterDatabase _instance;

    /// <summary>
    /// シングルトンインスタンス（Resourcesフォルダから読み込み）
    /// </summary>
    public static MasterDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<MasterDatabase>("MasterDatabase");
                if (_instance == null)
                {
                    Debug.LogError("MasterDatabaseが見つかりません！Resourcesフォルダに配置してください。");
                }
            }
            return _instance;
        }
    }

    [Header("アイテムマスターデータ")]
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();

    [Header("設備マスターデータ")]
    [SerializeField] private List<FacilityData> allFacilities = new List<FacilityData>();

    #region アイテムマスター

    /// <summary>
    /// すべてのアイテムデータを取得
    /// </summary>
    public List<ItemData> GetAllItemData()
    {
        return new List<ItemData>(allItems);
    }

    /// <summary>
    /// IDでアイテムデータを取得
    /// </summary>
    public ItemData GetItemData(string itemId)
    {
        return allItems.Find(item => item.itemId == itemId);
    }

    /// <summary>
    /// タイプ別にアイテムデータを取得
    /// </summary>
    public List<ItemData> GetItemDataByType(ItemType type)
    {
        return allItems.Where(item => item.type == type).ToList();
    }

    /// <summary>
    /// アイテムデータを追加（エディタまたは実行時）
    /// </summary>
    public void AddItemData(ItemData itemData)
    {
        if (!allItems.Contains(itemData))
        {
            allItems.Add(itemData);
            Debug.Log($"アイテムデータ「{itemData.itemName}」を追加しました。");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else
        {
            Debug.LogWarning($"アイテムデータ「{itemData.itemName}」は既に存在します。");
        }
    }

    /// <summary>
    /// アイテムデータを削除
    /// </summary>
    public void RemoveItemData(ItemData itemData)
    {
        if (allItems.Remove(itemData))
        {
            Debug.Log($"アイテムデータ「{itemData.itemName}」を削除しました。");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    /// <summary>
    /// IDでアイテムデータを削除
    /// </summary>
    public void RemoveItemDataById(string itemId)
    {
        ItemData itemData = GetItemData(itemId);
        if (itemData != null)
        {
            RemoveItemData(itemData);
        }
    }

    #endregion

    #region 設備マスター

    /// <summary>
    /// すべての設備データを取得
    /// </summary>
    public List<FacilityData> GetAllFacilityData()
    {
        return new List<FacilityData>(allFacilities);
    }

    /// <summary>
    /// IDで設備データを取得
    /// </summary>
    public FacilityData GetFacilityData(string facilityId)
    {
        return allFacilities.Find(facility => facility.facilityId == facilityId);
    }

    /// <summary>
    /// タイプ別に設備データを取得
    /// </summary>
    public List<FacilityData> GetFacilityDataByType(FacilityType type)
    {
        return allFacilities.Where(facility => facility.type == type).ToList();
    }

    /// <summary>
    /// 設備データを追加（エディタまたは実行時）
    /// </summary>
    public void AddFacilityData(FacilityData facilityData)
    {
        if (!allFacilities.Contains(facilityData))
        {
            allFacilities.Add(facilityData);
            Debug.Log($"設備データ「{facilityData.facilityName}」を追加しました。");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else
        {
            Debug.LogWarning($"設備データ「{facilityData.facilityName}」は既に存在します。");
        }
    }

    /// <summary>
    /// 設備データを削除
    /// </summary>
    public void RemoveFacilityData(FacilityData facilityData)
    {
        if (allFacilities.Remove(facilityData))
        {
            Debug.Log($"設備データ「{facilityData.facilityName}」を削除しました。");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    /// <summary>
    /// IDで設備データを削除
    /// </summary>
    public void RemoveFacilityDataById(string facilityId)
    {
        FacilityData facilityData = GetFacilityData(facilityId);
        if (facilityData != null)
        {
            RemoveFacilityData(facilityData);
        }
    }

    #endregion

    #region ユーティリティ

    /// <summary>
    /// データベースの統計情報を表示
    /// </summary>
    public void ShowStats()
    {
        Debug.Log($"=== MasterDatabase 統計 ===");
        Debug.Log($"アイテム総数: {allItems.Count}");
        Debug.Log($"設備総数: {allFacilities.Count}");
    }

    #endregion
}
