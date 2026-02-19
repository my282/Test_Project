using UnityEngine;

/// <summary>
/// MasterDatabaseの使用例
/// </summary>
public class MasterDatabaseExample : MonoBehaviour
{
    private void Start()
    {
        DemoMasterDatabase();
    }

    private void DemoMasterDatabase()
    {
        Debug.Log("=== MasterDatabase 使用例 ===");

        // すべてのアイテムデータを取得
        var allItems = MasterDatabase.Instance.GetAllItemData();
        Debug.Log($"登録されているアイテム数: {allItems.Count}");

        foreach (var itemData in allItems)
        {
            Debug.Log($"- {itemData.itemName} (ID: {itemData.itemId}, Type: {itemData.type})");
        }

        // 特定のアイテムデータを取得
        ItemData item1Data = MasterDatabase.Instance.GetItemData("item1");
        if (item1Data != null)
        {
            Debug.Log($"\nitem1の詳細:");
            Debug.Log($"名前: {item1Data.itemName}");
            Debug.Log($"説明: {item1Data.description}");
            Debug.Log($"価格: {item1Data.basePrice}");
        }

        // すべての設備データを取得
        var allFacilities = MasterDatabase.Instance.GetAllFacilityData();
        Debug.Log($"\n登録されている設備数: {allFacilities.Count}");

        foreach (var facilityData in allFacilities)
        {
            Debug.Log($"- {facilityData.facilityName} (ID: {facilityData.facilityId}, Type: {facilityData.type})");
        }

        // マスターデータからプレイヤーのインベントリにアイテムを追加
        if (item1Data != null)
        {
            GameDatabase.Instance.AddItem(
                item1Data.itemId,
                item1Data.itemName,
                item1Data.description,
                1,
                item1Data.type,
                item1Data.icon
            );
        }

        // 統計情報を表示
        MasterDatabase.Instance.ShowStats();
    }

    // マスターデータを使ってアイテムを購入する例
    public void BuyItemFromMaster(string itemId)
    {
        ItemData itemData = MasterDatabase.Instance.GetItemData(itemId);
        if (itemData != null)
        {
            if (GameDatabase.Instance.SpendMoney(itemData.basePrice))
            {
                GameDatabase.Instance.AddItem(
                    itemData.itemId,
                    itemData.itemName,
                    itemData.description,
                    1,
                    itemData.type,
                    itemData.icon
                );
                Debug.Log($"{itemData.itemName}を購入しました！");
            }
        }
        else
        {
            Debug.LogError($"アイテムID「{itemId}」が見つかりません。");
        }
    }

    // マスターデータを使って設備を解放する例（コスト支払い込み）
    public void UnlockFacilityFromMaster(string facilityId)
    {
        FacilityData facilityData = MasterDatabase.Instance.GetFacilityData(facilityId);
        if (facilityData != null)
        {
            // 必要なコストを表示
            Debug.Log($"=== {facilityData.facilityName} 解放コスト ===");
            if (facilityData.unlockMoneyCost > 0)
            {
                Debug.Log($"お金: {facilityData.unlockMoneyCost}");
            }
            foreach (var itemCost in facilityData.unlockItemCosts)
            {
                ItemData itemData = MasterDatabase.Instance.GetItemData(itemCost.itemId);
                string itemName = itemData != null ? itemData.itemName : itemCost.itemId;
                Debug.Log($"{itemName}: {itemCost.quantity}個");
            }

            // コスト支払いで解放
            if (GameDatabase.Instance.UnlockFacilityWithCost(facilityId))
            {
                Debug.Log($"{facilityData.facilityName}の解放に成功しました！");
            }
            else
            {
                Debug.Log($"{facilityData.facilityName}の解放に失敗しました。");
            }
        }
        else
        {
            Debug.LogError($"設備ID「{facilityId}」が見つかりません。");
        }
    }

    // 設備をアップグレードする例
    public void UpgradeFacilityExample(string facilityId)
    {
        if (GameDatabase.Instance.UpgradeFacilityWithCost(facilityId))
        {
            Debug.Log("アップグレードに成功しました！");
        }
        else
        {
            Debug.Log("アップグレードに失敗しました。");
        }
    }
}
