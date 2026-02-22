using UnityEngine;
using TMPro;

/// <summary>
/// プレイヤーの所持するアイテムの個数を表示するスクリプト
/// アイテム数の変化を検知して自動的に更新します
/// </summary>
public class ItemCountDisplay : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("表示するアイテムのID")]
    [SerializeField] private string itemId;

    [Header("UI参照")]
    [Tooltip("アイテム数を表示するTextMeshProUGUIコンポーネント")]
    [SerializeField] private TextMeshProUGUI itemCountText;

    [Header("表示フォーマット")]
    [Tooltip("表示フォーマット（{0}にアイテム数が入ります）")]
    [SerializeField] private string displayFormat = "x {0}";

    private void OnEnable()
    {
        // GameDatabaseのアイテム変更イベントを購読
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnItemsChanged += UpdateItemCount;
        }

        // 初回表示を更新
        UpdateItemCount();
    }

    private void OnDisable()
    {
        // イベントの購読を解除
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnItemsChanged -= UpdateItemCount;
        }
    }

    /// <summary>
    /// アイテム数を更新する
    /// </summary>
    private void UpdateItemCount()
    {
        if (itemCountText == null)
        {
            Debug.LogWarning($"ItemCountDisplay: TextMeshProUGUIが設定されていません。");
            return;
        }

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning($"ItemCountDisplay: itemIdが設定されていません。");
            itemCountText.text = "未設定";
            return;
        }

        // GameDatabaseからアイテム数を取得
        int itemCount = GameDatabase.Instance.GetItemQuantity(itemId);

        // テキストを更新
        itemCountText.text = string.Format(displayFormat, itemCount);
    }

    /// <summary>
    /// インスペクターから表示するアイテムIDを設定
    /// </summary>
    public void SetItemId(string newItemId)
    {
        itemId = newItemId;
        UpdateItemCount();
    }

    /// <summary>
    /// 表示フォーマットを設定
    /// </summary>
    public void SetDisplayFormat(string format)
    {
        displayFormat = format;
        UpdateItemCount();
    }
}
