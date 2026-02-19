using UnityEngine;

/// <summary>
/// HubInventoryUIのテスト用スクリプト
/// ボタンやキー入力でアイテムを追加/削除してUIの自動更新をテストする
/// </summary>
public class HubInventoryUITester : MonoBehaviour
{
    [Header("テスト設定")]
    [Tooltip("テスト用のアイコン画像（オプション）")]
    public Sprite testItemIcon;
    
    private int testItemCounter = 0;
    
    private void Update()
    {
        // キーボードショートカットでテスト
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddTestItem();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RemoveTestItem();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddMultipleTestItems();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ClearAllTestItems();
        }
    }
    
    /// <summary>
    /// テストアイテムを1つ追加
    /// </summary>
    public void AddTestItem()
    {
        testItemCounter++;
        string itemId = $"test_item_{testItemCounter}";
        string itemName = $"テストアイテム {testItemCounter}";
        
        GameDatabase.Instance.AddItem(
            itemId,
            itemName,
            "これはテスト用のアイテムです",
            Random.Range(1, 10),
            (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length)
        );
        
        Debug.Log($"テストアイテムを追加: {itemName}");
    }
    
    /// <summary>
    /// 既存のアイテムを削除
    /// </summary>
    public void RemoveTestItem()
    {
        var items = GameDatabase.Instance.GetAllItems();
        if (items.Count > 0)
        {
            Item itemToRemove = items[0];
            int removeAmount = Mathf.Min(itemToRemove.quantity, Random.Range(1, 4));
            
            GameDatabase.Instance.RemoveItem(itemToRemove.itemId, removeAmount);
            Debug.Log($"アイテムを削除: {itemToRemove.itemName} x{removeAmount}");
        }
        else
        {
            Debug.Log("削除するアイテムがありません");
        }
    }
    
    /// <summary>
    /// 複数のテストアイテムを追加
    /// </summary>
    public void AddMultipleTestItems()
    {
        // 消耗品
        GameDatabase.Instance.AddItem("potion_hp", "HPポーション", "HPを50回復", 5, ItemType.Consumable);
        GameDatabase.Instance.AddItem("potion_mp", "MPポーション", "MPを30回復", 3, ItemType.Consumable);
        
        // 装備
        GameDatabase.Instance.AddItem("sword_iron", "鉄の剣", "攻撃力+10", 1, ItemType.Equipment);
        GameDatabase.Instance.AddItem("shield_wood", "木の盾", "防御力+5", 1, ItemType.Equipment);
        
        // 素材
        GameDatabase.Instance.AddItem("wood", "木材", "建築素材", 20, ItemType.Material);
        GameDatabase.Instance.AddItem("stone", "石材", "建築素材", 15, ItemType.Material);
        GameDatabase.Instance.AddItem("iron_ore", "鉄鉱石", "精錬で鉄インゴットに", 10, ItemType.Material);
        
        // 重要アイテム
        GameDatabase.Instance.AddItem("key_gold", "金の鍵", "特別な扉を開く", 1, ItemType.KeyItem);
        
        Debug.Log("複数のテストアイテムを追加しました");
    }
    
    /// <summary>
    /// すべてのテストアイテムをクリア
    /// </summary>
    public void ClearAllTestItems()
    {
        var items = GameDatabase.Instance.GetAllItems();
        int count = items.Count;
        
        foreach (var item in items)
        {
            GameDatabase.Instance.RemoveItem(item.itemId, item.quantity);
        }
        
        Debug.Log($"{count}個のアイテムをクリアしました");
    }
    
    /// <summary>
    /// 既存のアイテムに数量を追加
    /// </summary>
    public void AddQuantityToExistingItem()
    {
        var items = GameDatabase.Instance.GetAllItems();
        if (items.Count > 0)
        {
            Item item = items[Random.Range(0, items.Count)];
            int addAmount = Random.Range(1, 5);
            
            GameDatabase.Instance.AddItem(
                item.itemId,
                item.itemName,
                item.description,
                addAmount,
                item.type
            );
            
            Debug.Log($"{item.itemName} に {addAmount}個 追加しました");
        }
        else
        {
            Debug.Log("アイテムがありません");
        }
    }
    
    private void OnGUI()
    {
        // 画面左上にテスト用のボタンを表示
        float x = 10f;
        float y = 10f;
        float width = 250f;
        float height = 30f;
        float spacing = 35f;
        
        GUI.Box(new Rect(x - 5, y - 5, width + 10, spacing * 6 + 10), "インベントリUIテスト");
        
        if (GUI.Button(new Rect(x, y, width, height), "1: テストアイテム追加"))
        {
            AddTestItem();
        }
        
        if (GUI.Button(new Rect(x, y + spacing, width, height), "2: アイテム削除"))
        {
            RemoveTestItem();
        }
        
        if (GUI.Button(new Rect(x, y + spacing * 2, width, height), "3: 複数アイテム追加"))
        {
            AddMultipleTestItems();
        }
        
        if (GUI.Button(new Rect(x, y + spacing * 3, width, height), "4: 全アイテムクリア"))
        {
            ClearAllTestItems();
        }
        
        if (GUI.Button(new Rect(x, y + spacing * 4, width, height), "既存アイテムに数量追加"))
        {
            AddQuantityToExistingItem();
        }
        
        GUI.Label(new Rect(x, y + spacing * 5, width, height), 
            $"現在のアイテム数: {GameDatabase.Instance.GetAllItems().Count}");
    }
}
