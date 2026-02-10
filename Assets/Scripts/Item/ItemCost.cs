using System;
using UnityEngine;

/// <summary>
/// アイテムコストのデータ構造
/// </summary>
[Serializable]
public class ItemCost
{
    public string itemId;           // 必要なアイテムのID
    public int quantity;            // 必要な数量

    public ItemCost(string id, int qty)
    {
        itemId = id;
        quantity = qty;
    }
}
