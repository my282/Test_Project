using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class change : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Image targetImage;  // 画像を差し替える対象のImage
    
    [Header("画像設定")]
    [SerializeField] private Sprite imageWhenLocked;    // phishing_siteを持っていない時の画像
    [SerializeField] private Sprite imageWhenUnlocked;  // phishing_siteを持っている時の画像
    
    private bool hasChecked = false;  // 初回チェック済みフラグ
    
    void Start()
    {
        // Imageが設定されていない場合は自動取得を試みる
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
        
        // 初期状態を設定
        UpdateImage();
        
        // GameDatabaseのイベントを購読（設備が変更されたら確認）
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnAssetsChanged += UpdateImage;
        }
    }
    
    void OnDestroy()
    {
        // イベント購読を解除
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnAssetsChanged -= UpdateImage;
        }
    }
    
    /// <summary>
    /// 画像を更新
    /// </summary>
    void UpdateImage()
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
            // 所持している場合はunlocked画像に差し替え
            if (imageWhenUnlocked != null)
            {
                targetImage.sprite = imageWhenUnlocked;
                
                // 初めて差し替えた時のログ
                if (!hasChecked)
                {
                    Debug.Log("✅ phishing_siteを入手したため、画像を差し替えました");
                    hasChecked = true;
                }
            }
            else
            {
                Debug.LogWarning("imageWhenUnlockedが設定されていません");
            }
        }
        else
        {
            // 所持していない場合はlocked画像に差し替え
            if (imageWhenLocked != null)
            {
                targetImage.sprite = imageWhenLocked;
            }
            else
            {
                Debug.LogWarning("imageWhenLockedが設定されていません");
            }
        }
    }
    
    /// <summary>
    /// 手動で画像を更新（デバッグ用）
    /// </summary>
    [ContextMenu("画像を更新")]
    public void ForceUpdate()
    {
        UpdateImage();
    }
}
