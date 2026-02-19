using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class overimage : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Image targetImage;  // 表示/非表示を切り替える画像
    
    [Header("表示設定")]
    [SerializeField] private bool useAlpha = true;  // true: alpha値で制御、false: enabled で制御
    
    private bool hasChecked = false;  // 初回チェック済みフラグ
    
    void Start()
    {
        // Imageが設定されていない場合は自動取得を試みる
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
        
        // 初期状態を設定
        UpdateVisibility();
        
        // GameDatabaseのイベントを購読（設備が変更されたら確認）
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnAssetsChanged += UpdateVisibility;
        }
    }
    
    void OnDestroy()
    {
        // イベント購読を解除
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnAssetsChanged -= UpdateVisibility;
        }
    }
    
    /// <summary>
    /// 表示/非表示を更新
    /// </summary>
    void UpdateVisibility()
    {
        if (targetImage == null)
        {
            Debug.LogWarning("targetImageが設定されていません");
            return;
        }
        
        // phishing_siteを所持しているか確認
        bool hasFacility = GameDatabase.Instance.HasFacility("phishing_site");
        
        if (hasFacility)
        {
            // 所持している場合は表示
            ShowImage();
            
            // 初めて表示された時のログ
            if (!hasChecked)
            {
                Debug.Log("✅ phishing_siteを入手したため、画像を表示しました");
                hasChecked = true;
            }
        }
        else
        {
            // 所持していない場合は非表示
            HideImage();
        }
    }
    
    /// <summary>
    /// 画像を表示
    /// </summary>
    void ShowImage()
    {
        if (useAlpha)
        {
            // alpha値を1にして表示
            Color color = targetImage.color;
            color.a = 1f;
            targetImage.color = color;
        }
        else
        {
            // enabledをtrueにして表示
            targetImage.enabled = true;
        }
    }
    
    /// <summary>
    /// 画像を非表示
    /// </summary>
    void HideImage()
    {
        if (useAlpha)
        {
            // alpha値を0にして透明化
            Color color = targetImage.color;
            color.a = 0f;
            targetImage.color = color;
        }
        else
        {
            // enabledをfalseにして非表示
            targetImage.enabled = false;
        }
    }
    
    /// <summary>
    /// 手動で表示を更新（デバッグ用）
    /// </summary>
    [ContextMenu("表示状態を更新")]
    public void ForceUpdate()
    {
        UpdateVisibility();
    }
}
