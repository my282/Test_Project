using UnityEngine;
using TMPro;

/// <summary>
/// 永続表示される情報の基底クラス
/// 総資産表示など、今後追加される表示要素の基本型
/// </summary>
public abstract class PersistentInfoDisplay : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] protected TextMeshProUGUI displayText;
    
    [Header("表示設定")]
    [SerializeField] protected string displayPrefix = "";
    [SerializeField] protected Color textColor = Color.white;
    
    protected virtual void Start()
    {
        InitializeDisplay();
        RegisterToPersistentUI();
    }

    protected virtual void OnDestroy()
    {
        UnregisterFromPersistentUI();
    }

    /// <summary>
    /// 表示の初期化
    /// </summary>
    protected virtual void InitializeDisplay()
    {
        if (displayText != null)
        {
            displayText.color = textColor;
        }
        UpdateDisplay();
    }

    /// <summary>
    /// PersistentUIManagerに登録
    /// </summary>
    protected virtual void RegisterToPersistentUI()
    {
        if (PersistentUIManager.Instance != null)
        {
            PersistentUIManager.Instance.RegisterDisplay(GetDisplayKey(), gameObject);
        }
    }

    /// <summary>
    /// PersistentUIManagerから登録解除
    /// </summary>
    protected virtual void UnregisterFromPersistentUI()
    {
        if (PersistentUIManager.Instance != null)
        {
            PersistentUIManager.Instance.UnregisterDisplay(GetDisplayKey());
        }
    }

    /// <summary>
    /// 表示を更新（派生クラスで実装）
    /// </summary>
    protected abstract void UpdateDisplay();

    /// <summary>
    /// 表示キーを取得（派生クラスで実装）
    /// </summary>
    protected abstract string GetDisplayKey();

    /// <summary>
    /// テキストを設定
    /// </summary>
    protected void SetText(string text)
    {
        if (displayText != null)
        {
            displayText.text = displayPrefix + text;
        }
    }
}
