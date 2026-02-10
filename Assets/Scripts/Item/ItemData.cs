using UnityEngine;

/// <summary>
/// アイテムのマスターデータ（ScriptableObject）
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("基本情報")]
    public string itemId;           // アイテムID
    public string itemName;         // アイテム名
    [TextArea(3, 5)]
    public string description;      // 説明
    public Sprite icon;             // アイコン画像
    public ItemType type;           // アイテムタイプ

    [Header("追加情報")]
    public int maxStackSize = 99;   // 最大スタック数
    public int basePrice = 0;       // 基本価格
    public bool isConsumable = false; // 消耗品かどうか

    /// <summary>
    /// Itemインスタンスを生成
    /// </summary>
    public Item CreateItem(int quantity = 1)
    {
        Item item = new Item(itemId, itemName, description, quantity, type);
        item.icon = icon;
        return item;
    }
}
