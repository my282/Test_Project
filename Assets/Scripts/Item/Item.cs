using System;
using UnityEngine;

/// <summary>
/// アイテムのデータ構造
/// </summary>
[Serializable]
public class Item
{
    public string itemId;           // アイテムID
    public string itemName;         // アイテム名
    public string description;      // 説明
    public int quantity;            // 所持数
    public Sprite icon;             // アイコン画像
    public ItemType type;           // アイテムタイプ

    public Item(string id, string name, string desc, int qty, ItemType itemType, Sprite itemIcon = null)
    {
        itemId = id;
        itemName = name;
        description = desc;
        quantity = qty;
        type = itemType;
        icon = itemIcon;
    }
}

/// <summary>
/// アイテムの種類
/// </summary>
public enum ItemType
{
    Consumable,     // 消耗品
    Equipment,      // 装備
    Material,       // 素材
    KeyItem,        // 重要アイテム
    Other           // その他
}
