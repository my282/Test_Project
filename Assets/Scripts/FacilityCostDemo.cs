using UnityEngine;

/// <summary>
/// 設備コストシステムのデモ
/// </summary>
public class FacilityCostDemo : MonoBehaviour
{
    private void Start()
    {
        DemoFacilityCost();
    }

    private void DemoFacilityCost()
    {
        Debug.Log("=== 設備コストシステム デモ ===");

        // プレイヤーに初期リソースを設定
        GameDatabase.Instance.SetMoney(5000);
        GameDatabase.Instance.AddItem("item1", "木材", "建築材料", 10, ItemType.Material);
        GameDatabase.Instance.AddItem("item2", "鉄鉱石", "建築材料", 5, ItemType.Material);

        Debug.Log($"\n初期状態:");
        Debug.Log($"所持金: {GameDatabase.Instance.GetMoney()}");
        Debug.Log($"木材: {GameDatabase.Instance.GetItemQuantity("item1")}個");
        Debug.Log($"鉄鉱石: {GameDatabase.Instance.GetItemQuantity("item2")}個");

        // facility1のコストを確認
        FacilityData facility1 = MasterDatabase.Instance.GetFacilityData("facility1");
        if (facility1 != null)
        {
            Debug.Log($"\n{facility1.facilityName}の解放コスト:");
            Debug.Log($"- お金: {facility1.unlockMoneyCost}");
            foreach (var itemCost in facility1.unlockItemCosts)
            {
                ItemData itemData = MasterDatabase.Instance.GetItemData(itemCost.itemId);
                string itemName = itemData != null ? itemData.itemName : itemCost.itemId;
                Debug.Log($"- {itemName}: {itemCost.quantity}個");
            }

            // 解放可能かチェック
            bool canAfford = GameDatabase.Instance.CanAffordFacilityUnlock(facility1);
            Debug.Log($"\n解放可能: {canAfford}");

            if (canAfford)
            {
                // 設備を解放
                if (GameDatabase.Instance.UnlockFacilityWithCost("facility1"))
                {
                    Debug.Log($"\n{facility1.facilityName}を解放しました！");
                    
                    Debug.Log($"\n解放後の状態:");
                    Debug.Log($"所持金: {GameDatabase.Instance.GetMoney()}");
                    Debug.Log($"木材: {GameDatabase.Instance.GetItemQuantity("item1")}個");
                    Debug.Log($"鉄鉱石: {GameDatabase.Instance.GetItemQuantity("item2")}個");
                }
            }
        }

        // アップグレードのデモ
        Debug.Log("\n=== アップグレードデモ ===");
        if (facility1 != null && facility1.upgradeItemCosts.Count > 0)
        {
            Debug.Log($"{facility1.facilityName}のアップグレードコスト:");
            Debug.Log($"- お金: {facility1.upgradeMoneyCost}");
            foreach (var itemCost in facility1.upgradeItemCosts)
            {
                ItemData itemData = MasterDatabase.Instance.GetItemData(itemCost.itemId);
                string itemName = itemData != null ? itemData.itemName : itemCost.itemId;
                Debug.Log($"- {itemName}: {itemCost.quantity}個");
            }

            bool canUpgrade = GameDatabase.Instance.CanAffordFacilityUpgrade(facility1);
            Debug.Log($"アップグレード可能: {canUpgrade}");

            if (canUpgrade)
            {
                GameDatabase.Instance.UpgradeFacilityWithCost("facility1");
            }
        }
    }
}
