using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class display_level_PS : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI levelText;  // レベル表示用テキスト
    
    [Header("表示フォーマット")]
    [SerializeField] private string displayFormat = "Lv.{0}";  // 表示フォーマット（{0}がレベルに置き換わる）
    
    void Start()
    {
        // TextMeshProが設定されていない場合は自動取得を試みる
        if (levelText == null)
        {
            levelText = GetComponent<TextMeshProUGUI>();
        }
        
        // 初期表示
        UpdateLevelDisplay();
        
        // GameDatabaseのイベントを購読
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnAssetsChanged += UpdateLevelDisplay;
        }
    }
    
    void OnDestroy()
    {
        // イベント購読を解除
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnAssetsChanged -= UpdateLevelDisplay;
        }
    }
    
    /// <summary>
    /// レベル表示を更新
    /// </summary>
    void UpdateLevelDisplay()
    {
        if (levelText == null)
        {
            Debug.LogWarning("levelTextが設定されていません");
            return;
        }
        
        // phishing_siteの情報を取得
        Facility facility = GameDatabase.Instance.GetFacility("phishing_site");
        
        if (facility != null && facility.isUnlocked)
        {
            // レベルを表示
            levelText.text = string.Format(displayFormat, facility.level);
        }
        else
        {
            // 未解放の場合は「-」や「???」などを表示
            levelText.text = "Not yet available";
        }
    }
    
    /// <summary>
    /// 手動で表示を更新（デバッグ用）
    /// </summary>
    [ContextMenu("レベル表示を更新")]
    public void ForceUpdate()
    {
        UpdateLevelDisplay();
    }
}
