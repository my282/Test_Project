using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// HubPageのインベントリUI管理
/// GameDatabaseのアイテム変更を自動検知して表示を更新する
/// </summary>
public class HubInventoryUIManager : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("アイテムスロットを配置する親オブジェクト")]
    public Transform inventoryContainer;
    
    [Tooltip("アイテムスロットのプレハブ")]
    public GameObject itemSlotPrefab;
    
    [Header("レイアウト設定")]
    [Tooltip("グリッドレイアウトを使用する場合はtrue")]
    public bool useGridLayout = true;
    
    [Tooltip("1行あたりの列数（グリッドレイアウト時）")]
    [Range(1, 10)]
    public int columns = 4;
    
    [Tooltip("セルのサイズ")]
    public Vector2 cellSize = new Vector2(150f, 150f);
    
    [Tooltip("セル間のスペース")]
    public Vector2 spacing = new Vector2(10f, 10f);
    
    [Header("フィルター設定")]
    [Tooltip("特定のアイテムタイプのみ表示する（Noneの場合は全て表示）")]
    public ItemType filterType = ItemType.Other; // デフォルトではフィルターなし
    
    [Tooltip("アイテムタイプでフィルタリングする")]
    public bool enableTypeFilter = false;
    
    [Header("デフォルトアイコン")]
    [Tooltip("アイテムにアイコンがない場合のデフォルトアイコン")]
    public Sprite defaultIcon;
    
    // アイテムスロットのリスト
    private List<InventoryItemSlot> itemSlots = new List<InventoryItemSlot>();
    
    private void OnEnable()
    {
        // イベントをリッスン
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnItemsChanged += RefreshInventory;
        }
        
        // 初回表示
        RefreshInventory();
    }
    
    private void OnDisable()
    {
        // イベント登録解除
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnItemsChanged -= RefreshInventory;
        }
    }
    
    private void Start()
    {
        // コンテナの設定を確認
        if (inventoryContainer == null)
        {
            Debug.LogWarning("Inventory Container が設定されていません。");
            return;
        }
        
        // グリッドレイアウトの設定
        if (useGridLayout)
        {
            SetupGridLayout();
        }
        
        // 初回表示
        RefreshInventory();
    }
    
    /// <summary>
    /// グリッドレイアウトコンポーネントを設定
    /// </summary>
    private void SetupGridLayout()
    {
        GridLayoutGroup gridLayout = inventoryContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = inventoryContainer.gameObject.AddComponent<GridLayoutGroup>();
        }
        
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
    }
    
    /// <summary>
    /// インベントリ表示を更新
    /// </summary>
    public void RefreshInventory()
    {
        if (inventoryContainer == null)
        {
            Debug.LogWarning("Inventory Container が設定されていません。");
            return;
        }
        
        // 既存のスロットをクリア
        ClearInventory();
        
        // GameDatabaseからアイテムを取得
        List<Item> items;
        if (enableTypeFilter)
        {
            items = GameDatabase.Instance.GetItemsByType(filterType);
        }
        else
        {
            items = GameDatabase.Instance.GetAllItems();
        }
        
        // アイテムごとにスロットを作成
        foreach (Item item in items)
        {
            CreateItemSlot(item);
        }
    }
    
    /// <summary>
    /// アイテムスロットを作成
    /// </summary>
    private void CreateItemSlot(Item item)
    {
        GameObject slotObj;
        
        if (itemSlotPrefab != null)
        {
            // カスタムプレハブを使用
            slotObj = Instantiate(itemSlotPrefab, inventoryContainer);
        }
        else
        {
            // デフォルトのスロットを生成
            slotObj = CreateDefaultSlot();
            slotObj.transform.SetParent(inventoryContainer, false);
        }
        
        // スロットコンポーネントを取得または追加
        InventoryItemSlot slot = slotObj.GetComponent<InventoryItemSlot>();
        if (slot == null)
        {
            slot = slotObj.AddComponent<InventoryItemSlot>();
        }
        
        // アイテム情報を設定
        Sprite icon = item.icon != null ? item.icon : defaultIcon;
        slot.SetupItem(item, icon);
        
        itemSlots.Add(slot);
    }
    
    /// <summary>
    /// デフォルトのアイテムスロットを作成
    /// </summary>
    private GameObject CreateDefaultSlot()
    {
        GameObject slotObj = new GameObject("ItemSlot");
        
        // Image (背景)
        Image bgImage = slotObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = cellSize;
        
        // アイコン用のImageオブジェクト
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.preserveAspect = true;
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0f, 15f);
        iconRect.sizeDelta = new Vector2(cellSize.x * 0.6f, cellSize.x * 0.6f);
        
        // アイテム名用のTextオブジェクト
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(slotObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 14;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.enableWordWrapping = true;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 0.3f);
        nameRect.offsetMin = new Vector2(5f, 20f);
        nameRect.offsetMax = new Vector2(-5f, -5f);
        
        // 数量用のTextオブジェクト
        GameObject qtyObj = new GameObject("Quantity");
        qtyObj.transform.SetParent(slotObj.transform, false);
        TextMeshProUGUI qtyText = qtyObj.AddComponent<TextMeshProUGUI>();
        qtyText.fontSize = 16;
        qtyText.fontStyle = FontStyles.Bold;
        qtyText.alignment = TextAlignmentOptions.BottomRight;
        qtyText.color = Color.white;
        
        RectTransform qtyRect = qtyObj.GetComponent<RectTransform>();
        qtyRect.anchorMin = new Vector2(0f, 0f);
        qtyRect.anchorMax = new Vector2(1f, 0.3f);
        qtyRect.offsetMin = new Vector2(5f, 5f);
        qtyRect.offsetMax = new Vector2(-5f, 5f);
        
        return slotObj;
    }
    
    /// <summary>
    /// インベントリをクリア
    /// </summary>
    private void ClearInventory()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        itemSlots.Clear();
    }
    
    /// <summary>
    /// フィルタータイプを変更
    /// </summary>
    public void SetFilterType(ItemType type)
    {
        filterType = type;
        enableTypeFilter = true;
        RefreshInventory();
    }
    
    /// <summary>
    /// フィルターを解除
    /// </summary>
    public void ClearFilter()
    {
        enableTypeFilter = false;
        RefreshInventory();
    }
}
