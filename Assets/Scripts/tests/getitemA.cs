using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class get_itemA : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void get()
    {
        // MasterDatabaseから全アイテムを取得
        var allItemData = MasterDatabase.Instance.GetAllItemData();
        
        int addedCount = 0;
        
        // itemA以外のすべてのアイテムを追加
        foreach (var itemData in allItemData)
        {
            // itemAは除外
            if (itemData.itemId == "itemA")
            {
                continue;
            }
            
            // アイテムをGameDatabaseに追加（数量は1、アイコンも含む）
            GameDatabase.Instance.AddItem(
                itemData.itemId,
                itemData.itemName,
                itemData.description,
                1,
                itemData.type,
                itemData.icon
            );
            
            addedCount++;
        }
        
        Debug.Log($"itemAを除く全アイテム {addedCount}種類 を入手しました！");
    }
}