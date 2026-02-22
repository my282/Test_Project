using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class overimage : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Image targetImage;  // 表示/非表示を切り替える画像
    [SerializeField] private Text[] targetTexts;  // 表示/非表示を切り替えるText (UI Text)
    [SerializeField] private TextMeshProUGUI[] targetTMPTexts;  // 表示/非表示を切り替えるText (TextMeshPro)
    
    [Header("表示設定")]
    [SerializeField] private bool useAlpha = true;  // true: alpha値で制御、false: enabled で制御
    [SerializeField] private bool autoDetectChildTexts = true;  // 子要素のTextを自動検出
    
    private bool hasChecked = false;  // 初回チェック済みフラグ
    
    void Start()
    {
        // Imageが設定されていない場合は自動取得を試みる
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
        
        // 子要素のTextを自動検出
        if (autoDetectChildTexts)
        {
            if (targetTexts == null || targetTexts.Length == 0)
            {
                targetTexts = GetComponentsInChildren<Text>(true);
            }
            
            if (targetTMPTexts == null || targetTMPTexts.Length == 0)
            {
                targetTMPTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            }
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
            if (targetImage != null)
            {
                Color color = targetImage.color;
                color.a = 1f;
                targetImage.color = color;
            }
            
            // UI Textのalpha値を1にして表示
            if (targetTexts != null)
            {
                foreach (var text in targetTexts)
                {
                    if (text != null)
                    {
                        Color color = text.color;
                        color.a = 1f;
                        text.color = color;
                    }
                }
            }
            
            // TextMeshProのalpha値を1にして表示
            if (targetTMPTexts != null)
            {
                foreach (var text in targetTMPTexts)
                {
                    if (text != null)
                    {
                        Color color = text.color;
                        color.a = 1f;
                        text.color = color;
                    }
                }
            }
        }
        else
        {
            // enabledをtrueにして表示
            if (targetImage != null)
            {
                targetImage.enabled = true;
            }
            
            // UI Textのenabledをtrueにして表示
            if (targetTexts != null)
            {
                foreach (var text in targetTexts)
                {
                    if (text != null)
                    {
                        text.enabled = true;
                    }
                }
            }
            
            // TextMeshProのenabledをtrueにして表示
            if (targetTMPTexts != null)
            {
                foreach (var text in targetTMPTexts)
                {
                    if (text != null)
                    {
                        text.enabled = true;
                    }
                }
            }
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
            if (targetImage != null)
            {
                Color color = targetImage.color;
                color.a = 0f;
                targetImage.color = color;
            }
            
            // UI Textのalpha値を0にして透明化
            if (targetTexts != null)
            {
                foreach (var text in targetTexts)
                {
                    if (text != null)
                    {
                        Color color = text.color;
                        color.a = 0f;
                        text.color = color;
                    }
                }
            }
            
            // TextMeshProのalpha値を0にして透明化
            if (targetTMPTexts != null)
            {
                foreach (var text in targetTMPTexts)
                {
                    if (text != null)
                    {
                        Color color = text.color;
                        color.a = 0f;
                        text.color = color;
                    }
                }
            }
        }
        else
        {
            // enabledをfalseにして非表示
            if (targetImage != null)
            {
                targetImage.enabled = false;
            }
            
            // UI Textのenabledをfalseにして非表示
            if (targetTexts != null)
            {
                foreach (var text in targetTexts)
                {
                    if (text != null)
                    {
                        text.enabled = false;
                    }
                }
            }
            
            // TextMeshProのenabledをfalseにして非表示
            if (targetTMPTexts != null)
            {
                foreach (var text in targetTMPTexts)
                {
                    if (text != null)
                    {
                        text.enabled = false;
                    }
                }
            }
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
