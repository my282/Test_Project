using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class HubPageManager : MonoBehaviour
{
    [Header("ボタン設定")]
    [Tooltip("ページ遷移ボタンのリスト")]
    public List<PageButtonData> pageButtons = new List<PageButtonData>();
    
    [Header("レイアウト設定")]
    [Tooltip("ボタンを配置する親オブジェクト")]
    public Transform buttonContainer;
    
    [Tooltip("ボタン間のスペース")]
    [Range(0, 50)]
    public float buttonSpacing = 10f;
    
    [Tooltip("縦方向に配置")]
    public bool verticalLayout = true;
    
    [Header("プレハブ設定（オプション）")]
    [Tooltip("カスタムボタンプレハブ（未設定の場合は自動生成）")]
    public GameObject customButtonPrefab;
    
    private void Start()
    {
        CreateButtons();
    }
    
    /// <summary>
    /// ボタンを生成する
    /// </summary>
    private void CreateButtons()
    {
        if (buttonContainer == null)
        {
            Debug.LogWarning("Button Container が設定されていません。Canvas 配下に自動生成します。");
            CreateButtonContainer();
        }
        
        // 既存のボタンをクリア
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 各ボタンデータからボタンを生成
        float currentPosition = 0f;
        foreach (var buttonData in pageButtons)
        {
            CreateButton(buttonData, currentPosition);
            currentPosition += (verticalLayout ? buttonData.buttonHeight : buttonData.buttonWidth) + buttonSpacing;
        }
    }
    
    /// <summary>
    /// 個別のボタンを作成
    /// </summary>
    private void CreateButton(PageButtonData data, float position)
    {
        GameObject buttonObj;
        
        if (customButtonPrefab != null)
        {
            buttonObj = Instantiate(customButtonPrefab, buttonContainer);
        }
        else
        {
            buttonObj = CreateDefaultButton();
            buttonObj.transform.SetParent(buttonContainer, false);
        }
        
        // ボタンの名前設定
        buttonObj.name = $"Button_{data.sceneName}";
        
        // RectTransform設定
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(data.buttonWidth, data.buttonHeight);
        
        if (verticalLayout)
        {
            rectTransform.anchoredPosition = new Vector2(0, -position);
        }
        else
        {
            rectTransform.anchoredPosition = new Vector2(position, 0);
        }
        
        // Button コンポーネント取得と設定
        Button button = buttonObj.GetComponent<Button>();
        Image buttonImage = buttonObj.GetComponent<Image>();
        
        // 背景色設定
        if (buttonImage != null)
        {
            buttonImage.color = data.backgroundColor;
        }
        
        // テキスト設定
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null)
        {
            // TextMeshProが見つからない場合はText UIを探す
            Text legacyText = buttonObj.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.text = data.buttonText;
                legacyText.color = data.textColor;
                legacyText.fontSize = data.fontSize;
            }
        }
        else
        {
            buttonText.text = data.buttonText;
            buttonText.color = data.textColor;
            buttonText.fontSize = data.fontSize;
        }
        
        // クリックイベント設定
        button.onClick.AddListener(() => LoadScene(data.sceneName));
    }
    
    /// <summary>
    /// デフォルトのボタンプレハブを作成
    /// </summary>
    private GameObject CreateDefaultButton()
    {
        GameObject buttonObj = new GameObject("Button");
        
        // Buttonコンポーネント追加
        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.white;
        
        Button button = buttonObj.AddComponent<Button>();
        
        // テキスト子オブジェクト作成
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        // TextMeshProまたはTextコンポーネントを追加
        // まずTextMeshProを試す
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return buttonObj;
    }
    
    /// <summary>
    /// ボタンコンテナを自動生成
    /// </summary>
    private void CreateButtonContainer()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas が見つかりません。");
            return;
        }
        
        GameObject container = new GameObject("ButtonContainer");
        container.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = container.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        
        buttonContainer = container.transform;
    }
    
    /// <summary>
    /// シーンを読み込む
    /// </summary>
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("シーン名が設定されていません。");
            return;
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// エディタでボタンを再生成（エディタ専用）
    /// </summary>
    [ContextMenu("ボタンを再生成")]
    public void RegenerateButtons()
    {
        if (Application.isPlaying)
        {
            CreateButtons();
        }
        else
        {
            Debug.Log("ゲームを実行中にのみボタンを再生成できます。");
        }
    }
}
