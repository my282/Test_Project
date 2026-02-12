using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ゲーム全体のデータベース（シングルトン）
/// どこからでもアクセス可能なデータ管理クラス
/// </summary>
public class GameDatabase : MonoBehaviour
{
    private static GameDatabase _instance;

    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static GameDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameDatabase>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameDatabase");
                    _instance = go.AddComponent<GameDatabase>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("プレイヤーデータ")]
    [SerializeField] private int playerMoney = 0;                       // 所持金

    [Header("アイテムデータ")]
    [SerializeField] private List<Item> items = new List<Item>();      // アイテムリスト

    [Header("設備データ")]
    [SerializeField] private List<Facility> facilities = new List<Facility>();  // 設備リスト

    private void Awake()
    {
        // シングルトンの設定
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    #region 所持金関連

    /// <summary>
    /// 所持金を取得
    /// </summary>
    public int GetMoney()
    {
        return playerMoney;
    }

    /// <summary>
    /// 所持金を追加
    /// </summary>
    public void AddMoney(int amount)
    {
        playerMoney += amount;
        Debug.Log($"所持金を{amount}追加しました。現在の所持金: {playerMoney}");
    }

    /// <summary>
    /// 所持金を使用（減らす）
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (playerMoney >= amount)
        {
            playerMoney -= amount;
            Debug.Log($"所持金を{amount}使用しました。残りの所持金: {playerMoney}");
            return true;
        }
        else
        {
            Debug.LogWarning("所持金が足りません！");
            return false;
        }
    }

    /// <summary>
    /// 所持金を設定
    /// </summary>
    public void SetMoney(int amount)
    {
        playerMoney = amount;
    }

    #endregion

    #region アイテム関連

    /// <summary>
    /// すべてのアイテムを取得
    /// </summary>
    public List<Item> GetAllItems()
    {
        return new List<Item>(items);
    }

    /// <summary>
    /// アイテムを追加
    /// </summary>
    public void AddItem(string itemId, string itemName, string description, int quantity, ItemType type)
    {
        Item existingItem = items.Find(i => i.itemId == itemId);
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
            Debug.Log($"{itemName}を{quantity}個追加しました。合計: {existingItem.quantity}個");
        }
        else
        {
            Item newItem = new Item(itemId, itemName, description, quantity, type);
            items.Add(newItem);
            Debug.Log($"新しいアイテム「{itemName}」を{quantity}個取得しました。");
        }
    }

    /// <summary>
    /// アイテムを削除（使用）
    /// </summary>
    public bool RemoveItem(string itemId, int quantity)
    {
        Item item = items.Find(i => i.itemId == itemId);
        if (item != null && item.quantity >= quantity)
        {
            item.quantity -= quantity;
            Debug.Log($"{item.itemName}を{quantity}個使用しました。残り: {item.quantity}個");
            
            if (item.quantity <= 0)
            {
                items.Remove(item);
                Debug.Log($"{item.itemName}がなくなりました。");
            }
            return true;
        }
        else
        {
            Debug.LogWarning("アイテムが不足しています！");
            return false;
        }
    }

    /// <summary>
    /// 特定のアイテムを取得
    /// </summary>
    public Item GetItem(string itemId)
    {
        return items.Find(i => i.itemId == itemId);
    }

    /// <summary>
    /// アイテムの所持数を取得
    /// </summary>
    public int GetItemQuantity(string itemId)
    {
        Item item = GetItem(itemId);
        return item != null ? item.quantity : 0;
    }

    /// <summary>
    /// タイプ別にアイテムを取得
    /// </summary>
    public List<Item> GetItemsByType(ItemType type)
    {
        return items.Where(i => i.type == type).ToList();
    }

    #endregion

    #region 設備関連

    /// <summary>
    /// すべての設備を取得
    /// </summary>
    public List<Facility> GetAllFacilities()
    {
        return new List<Facility>(facilities);
    }

    /// <summary>
    /// 設備を追加
    /// </summary>
    public void AddFacility(string facilityId, string facilityName, string description, int level, bool isUnlocked, FacilityType type)
    {
        Facility existingFacility = facilities.Find(f => f.facilityId == facilityId);
        if (existingFacility == null)
        {
            Facility newFacility = new Facility(facilityId, facilityName, description, level, isUnlocked, type);
            facilities.Add(newFacility);
            Debug.Log($"新しい設備「{facilityName}」を追加しました。");
        }
        else
        {
            Debug.LogWarning($"設備「{facilityId}」は既に存在します。");
        }
    }

    /// <summary>
    /// 設備を解放
    /// </summary>
    public bool UnlockFacility(string facilityId)
    {
        Facility facility = facilities.Find(f => f.facilityId == facilityId);
        if (facility != null)
        {
            facility.isUnlocked = true;
            Debug.Log($"設備「{facility.facilityName}」を解放しました。");
            return true;
        }
        else
        {
            Debug.LogWarning($"設備「{facilityId}」が見つかりません。");
            return false;
        }
    }

    /// <summary>
    /// 設備を解放（コスト支払い込み）
    /// </summary>
    public bool UnlockFacilityWithCost(string facilityId)
    {
        FacilityData facilityData = MasterDatabase.Instance.GetFacilityData(facilityId);
        if (facilityData == null)
        {
            Debug.LogWarning($"設備データ「{facilityId}」が見つかりません。");
            return false;
        }

        // 既に解放済みかチェック
        Facility existingFacility = facilities.Find(f => f.facilityId == facilityId);
        if (existingFacility != null && existingFacility.isUnlocked)
        {
            Debug.LogWarning($"設備「{facilityData.facilityName}」は既に解放済みです。");
            return false;
        }

        // コストをチェック
        if (!CanAffordFacilityUnlock(facilityData))
        {
            Debug.LogWarning($"設備「{facilityData.facilityName}」を解放するためのコストが足りません。");
            return false;
        }

        // お金を消費
        if (facilityData.unlockMoneyCost > 0)
        {
            SpendMoney(facilityData.unlockMoneyCost);
        }

        // アイテムを消費
        foreach (var itemCost in facilityData.unlockItemCosts)
        {
            RemoveItem(itemCost.itemId, itemCost.quantity);
        }

        // 設備を追加または解放
        if (existingFacility != null)
        {
            existingFacility.isUnlocked = true;
            
            // 既存の設備の生成設定を更新（FacilityDataの設定を反映）
            existingFacility.productionConfig = facilityData.productionConfig;
        }
        else
        {
            // FacilityData.CreateFacility()を使用して生成設定も含めて作成
            Facility newFacility = facilityData.CreateFacility(level: 1, isUnlocked: true);
            facilities.Add(newFacility);
            Debug.Log($"新しい設備「{facilityData.facilityName}」を追加しました。");
        }

        Debug.Log($"設備「{facilityData.facilityName}」を解放しました！");
        return true;
    }

    /// <summary>
    /// 設備解放のコストを支払えるかチェック
    /// </summary>
    public bool CanAffordFacilityUnlock(FacilityData facilityData)
    {
        // お金をチェック
        if (playerMoney < facilityData.unlockMoneyCost)
        {
            Debug.Log($"お金が足りません。必要: {facilityData.unlockMoneyCost}, 所持: {playerMoney}");
            return false;
        }

        // アイテムをチェック
        foreach (var itemCost in facilityData.unlockItemCosts)
        {
            int currentQuantity = GetItemQuantity(itemCost.itemId);
            if (currentQuantity < itemCost.quantity)
            {
                ItemData itemData = MasterDatabase.Instance.GetItemData(itemCost.itemId);
                string itemName = itemData != null ? itemData.itemName : itemCost.itemId;
                Debug.Log($"{itemName}が足りません。必要: {itemCost.quantity}, 所持: {currentQuantity}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 設備をアップグレード
    /// </summary>
    public bool UpgradeFacility(string facilityId)
    {
        Facility facility = facilities.Find(f => f.facilityId == facilityId);
        if (facility != null && facility.isUnlocked)
        {
            facility.level++;
            Debug.Log($"設備「{facility.facilityName}」をレベル{facility.level}にアップグレードしました。");
            return true;
        }
        else
        {
            Debug.LogWarning($"設備をアップグレードできません。");
            return false;
        }
    }

    /// <summary>
    /// 設備をアップグレード（コスト支払い込み）
    /// </summary>
    public bool UpgradeFacilityWithCost(string facilityId)
    {
        FacilityData facilityData = MasterDatabase.Instance.GetFacilityData(facilityId);
        if (facilityData == null)
        {
            Debug.LogWarning($"設備データ「{facilityId}」が見つかりません。");
            return false;
        }

        Facility facility = facilities.Find(f => f.facilityId == facilityId);
        if (facility == null || !facility.isUnlocked)
        {
            Debug.LogWarning($"設備「{facilityData.facilityName}」が見つからないか、未解放です。");
            return false;
        }

        if (facility.level >= facilityData.maxLevel)
        {
            Debug.LogWarning($"設備「{facilityData.facilityName}」は既に最大レベルです。");
            return false;
        }

        // コストをチェック
        if (!CanAffordFacilityUpgrade(facilityData))
        {
            Debug.LogWarning($"設備「{facilityData.facilityName}」をアップグレードするためのコストが足りません。");
            return false;
        }

        // お金を消費
        if (facilityData.upgradeMoneyCost > 0)
        {
            SpendMoney(facilityData.upgradeMoneyCost);
        }

        // アイテムを消費
        foreach (var itemCost in facilityData.upgradeItemCosts)
        {
            RemoveItem(itemCost.itemId, itemCost.quantity);
        }

        // レベルアップ
        facility.level++;
        Debug.Log($"設備「{facilityData.facilityName}」をレベル{facility.level}にアップグレードしました！");
        return true;
    }

    /// <summary>
    /// 設備アップグレードのコストを支払えるかチェック
    /// </summary>
    public bool CanAffordFacilityUpgrade(FacilityData facilityData)
    {
        // お金をチェック
        if (playerMoney < facilityData.upgradeMoneyCost)
        {
            Debug.Log($"お金が足りません。必要: {facilityData.upgradeMoneyCost}, 所持: {playerMoney}");
            return false;
        }

        // アイテムをチェック
        foreach (var itemCost in facilityData.upgradeItemCosts)
        {
            int currentQuantity = GetItemQuantity(itemCost.itemId);
            if (currentQuantity < itemCost.quantity)
            {
                ItemData itemData = MasterDatabase.Instance.GetItemData(itemCost.itemId);
                string itemName = itemData != null ? itemData.itemName : itemCost.itemId;
                Debug.Log($"{itemName}が足りません。必要: {itemCost.quantity}, 所持: {currentQuantity}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 特定の設備を取得
    /// </summary>
    public Facility GetFacility(string facilityId)
    {
        return facilities.Find(f => f.facilityId == facilityId);
    }

    /// <summary>
    /// 特定の設備を所持しているか（解放済みか）確認
    /// 新規追加した関数
    /// </summary>
    public bool HasFacility(string facilityId)
    {
        Facility facility = GetFacility(facilityId);
        return facility != null && facility.isUnlocked;
    }

    /// <summary>
    /// 解放済みの設備のみを取得
    /// </summary>
    public List<Facility> GetUnlockedFacilities()
    {
        return facilities.Where(f => f.isUnlocked).ToList();
    }

    /// <summary>
    /// タイプ別に設備を取得
    /// </summary>
    public List<Facility> GetFacilitiesByType(FacilityType type)
    {
        return facilities.Where(f => f.type == type).ToList();
    }

    #endregion

    #region データの保存・読み込み

    /// <summary>
    /// すべてのデータをクリア
    /// </summary>
    public void ClearAllData()
    {
        playerMoney = 0;
        items.Clear();
        facilities.Clear();
        Debug.Log("すべてのデータをクリアしました。");
    }

    #endregion
}
