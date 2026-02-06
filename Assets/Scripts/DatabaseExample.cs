using UnityEngine;

/// <summary>
/// GameDatabaseの使用例
/// </summary>
public class DatabaseExample : MonoBehaviour
{
    private void Start()
    {
        // 使用例のデモ
        DemoDatabase();
    }

    private void DemoDatabase()
    {
        Debug.Log("=== GameDatabase 使用例 ===");

        // 所持金の操作
        GameDatabase.Instance.AddMoney(1000);
        Debug.Log($"現在の所持金: {GameDatabase.Instance.GetMoney()}");

        // アイテムの追加
        GameDatabase.Instance.AddItem("potion_001", "回復ポーション", "HPを50回復する", 5, ItemType.Consumable);
        GameDatabase.Instance.AddItem("sword_001", "鉄の剣", "攻撃力+10", 1, ItemType.Equipment);
        GameDatabase.Instance.AddItem("wood_001", "木材", "建築素材", 20, ItemType.Material);

        // アイテムの使用
        GameDatabase.Instance.RemoveItem("potion_001", 2);

        // すべてのアイテムを表示
        Debug.Log("=== 所持アイテム一覧 ===");
        foreach (var item in GameDatabase.Instance.GetAllItems())
        {
            Debug.Log($"{item.itemName} x{item.quantity} - {item.description}");
        }

        // 設備の追加
        GameDatabase.Instance.AddFacility("workshop_001", "作業場", "アイテムを製作できる", 1, true, FacilityType.Production);
        GameDatabase.Instance.AddFacility("warehouse_001", "倉庫", "アイテムを保管できる", 1, false, FacilityType.Storage);

        // 設備の解放
        GameDatabase.Instance.UnlockFacility("warehouse_001");

        // 設備のアップグレード
        GameDatabase.Instance.UpgradeFacility("workshop_001");

        // すべての設備を表示
        Debug.Log("=== 設備一覧 ===");
        foreach (var facility in GameDatabase.Instance.GetAllFacilities())
        {
            string status = facility.isUnlocked ? "解放済み" : "未解放";
            Debug.Log($"{facility.facilityName} (Lv.{facility.level}) - {status}");
        }
    }

    // 他のスクリプトからアクセスする例
    public void BuyItem()
    {
        int itemPrice = 100;
        if (GameDatabase.Instance.SpendMoney(itemPrice))
        {
            GameDatabase.Instance.AddItem("potion_001", "回復ポーション", "HPを50回復する", 1, ItemType.Consumable);
        }
    }

    public void CheckInventory()
    {
        int potionCount = GameDatabase.Instance.GetItemQuantity("potion_001");
        Debug.Log($"回復ポーションの所持数: {potionCount}");
    }
}
