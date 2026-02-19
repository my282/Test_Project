using UnityEngine;

/// <summary>
/// HubPageでインベントリをすぐに表示するための簡易スターター
/// シーンに追加するだけでテストアイテムが自動的に表示される
/// </summary>
public class HubInventoryQuickStart : MonoBehaviour
{
    [Header("自動テストアイテム追加")]
    [Tooltip("Start時に自動的にテストアイテムを追加する")]
    public bool addItemsOnStart = true;
    
    [Tooltip("追加するアイテムの種類")]
    public int numberOfItems = 8;
    
    private void Start()
    {
        if (addItemsOnStart)
        {
            AddSampleItems();
        }
    }
    
    /// <summary>
    /// サンプルアイテムを追加
    /// </summary>
    public void AddSampleItems()
    {
        Debug.Log("=== インベントリにサンプルアイテムを追加 ===");
        
        // 消耗品
        GameDatabase.Instance.AddItem("hp_potion", "HPポーション", "HPを50回復する", 10, ItemType.Consumable);
        GameDatabase.Instance.AddItem("mp_potion", "MPポーション", "MPを30回復する", 8, ItemType.Consumable);
        GameDatabase.Instance.AddItem("elixir", "エリクサー", "HP・MPを完全回復", 2, ItemType.Consumable);
        
        // 装備
        GameDatabase.Instance.AddItem("iron_sword", "鉄の剣", "攻撃力+15", 1, ItemType.Equipment);
        GameDatabase.Instance.AddItem("steel_shield", "鋼の盾", "防御力+20", 1, ItemType.Equipment);
        
        // 素材
        GameDatabase.Instance.AddItem("wood", "木材", "建築や製作に使う素材", 50, ItemType.Material);
        GameDatabase.Instance.AddItem("stone", "石材", "建築素材", 30, ItemType.Material);
        GameDatabase.Instance.AddItem("iron_ore", "鉄鉱石", "精錬して鉄インゴットに", 15, ItemType.Material);
        
        Debug.Log($"サンプルアイテムを追加しました。現在のアイテム数: {GameDatabase.Instance.GetAllItems().Count}");
    }
}
