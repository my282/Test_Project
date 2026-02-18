using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// インベントリ内の個別のアイテムスロット
/// アイコン、名前、数量を表示する
/// </summary>
public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI参照（自動取得）")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;
    
    // アイテムデータの参照
    private Item itemData;
    
    /// <summary>
    /// アイテム情報を設定して表示を更新
    /// </summary> 
    public void SetupItem(Item item, Sprite icon)
    {
        itemData = item;
        
        // UI参照を自動取得（未設定の場合）
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }
        }
        
        if (nameText == null)
        {
            Transform nameTransform = transform.Find("Name");
            if (nameTransform != null)
            {
                nameText = nameTransform.GetComponent<TextMeshProUGUI>();
            }
        }
        
        if (quantityText == null)
        {
            Transform qtyTransform = transform.Find("Quantity");
            if (qtyTransform != null)
            {
                quantityText = qtyTransform.GetComponent<TextMeshProUGUI>();
            }
        }
        
        // UIに反映
        UpdateDisplay(icon);
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay(Sprite icon)
    {
        if (itemData == null) return;
        
        // アイコンを設定
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }
        
        // アイテム名を設定
        if (nameText != null)
        {
            nameText.text = itemData.itemName;
        }
        
        // 数量を設定
        if (quantityText != null)
        {
            quantityText.text = $"x{itemData.quantity}";
        }
    }
    
    /// <summary>
    /// アイテムがクリックされた時の処理
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemData != null)
        {
            Debug.Log($"アイテムをクリック: {itemData.itemName} (ID: {itemData.itemId})");
            // 将来的に詳細表示などの機能をここに追加可能
        }
    }
    
    /// <summary>
    /// 現在のアイテムデータを取得
    /// </summary>
    public Item GetItemData()
    {
        return itemData;
    }
}
