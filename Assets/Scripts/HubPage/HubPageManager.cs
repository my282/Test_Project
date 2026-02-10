using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public enum LayoutMode
{
    Vertical,       // 縦方向に自動配置
    Horizontal,     // 横方向に自動配置
    Grid,           // グリッド状に配置
    Custom          // 各ボタンの個別設定を使用
}

[ExecuteAlways]
public class HubPageManager : MonoBehaviour
{
    [Header("ボタン設定")]
    [Tooltip("ページ遷移ボタンのリスト")]
    public List<PageButtonData> pageButtons = new List<PageButtonData>();
    
    [Header("レイアウト設定")]
    [Tooltip("ボタンを配置する親オブジェクト")]
    public Transform buttonContainer;
    
    [Tooltip("レイアウトモード")]
    public LayoutMode layoutMode = LayoutMode.Vertical;
    
    [Tooltip("ボタン間のスペース（自動配置時のみ）")]
    [Range(0, 50)]
    public float buttonSpacing = 10f;
    
    [Tooltip("開始位置（自動配置時）")]
    public Vector2 startPosition = Vector2.zero;
    
    [Header("グリッドレイアウト設定")]
    [Tooltip("1行あたりのボタン数（グリッドモード時）")]
    [Range(1, 10)]
    public int gridColumns = 3;
    
    [Tooltip("グリッドセルサイズ")]
    public Vector2 gridCellSize = new Vector2(320f, 70f);
    
    [Header("プレハブ設定（オプション）")]
    [Tooltip("カスタムボタンプレハブ（未設定の場合は自動生成）")]
    public GameObject customButtonPrefab;
    
    private void Start()
    {
        if (Application.isPlaying)
        {
            CreateButtons();
        }
    }
    
    private void OnEnable()
    {
        // エディタモードでもボタンを表示
        if (!Application.isPlaying)
        {
            CreateButtons();
        }
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
        
        // 既存のボタンをクリア（エディタとランタイムで異なる方法を使用）
        ClearExistingButtons();
        
        // 各ボタンデータからボタンを生成
        for (int i = 0; i < pageButtons.Count; i++)
        {
            CreateButton(pageButtons[i], i);
        }
    }
    
    /// <summary>
    /// 既存のボタンをクリア
    /// </summary>
    private void ClearExistingButtons()
    {
        if (buttonContainer == null) return;
        
        // 子オブジェクトのリストを作成（削除中にコレクションが変更されるのを防ぐ）
        var childrenToDestroy = new System.Collections.Generic.List<GameObject>();
        
        foreach (Transform child in buttonContainer)
        {
            childrenToDestroy.Add(child.gameObject);
        }
        
        // エディタモードとプレイモードで異なる削除方法を使用
        foreach (var child in childrenToDestroy)
        {
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(child);
#endif
            }
        }
    }
    
    /// <summary>
    /// 個別のボタンを作成
    /// </summary>
    private void CreateButton(PageButtonData data, int index)
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
        
        // 位置設定
        if (data.useCustomPosition)
        {
            // カスタム位置を使用
            SetAnchorPreset(rectTransform, data.anchorPreset);
            rectTransform.anchoredPosition = data.customPosition;
        }
        else
        {
            // 自動配置
            SetButtonPosition(rectTransform, data, index);
        }
        
        // Button コンポーネント取得と設定
        Button button = buttonObj.GetComponent<Button>();
        Image buttonImage = buttonObj.GetComponent<Image>();
        
        // 背景画像または背景色設定
        if (buttonImage != null)
        {
            if (data.backgroundSprite != null)
            {
                buttonImage.sprite = data.backgroundSprite;
                buttonImage.type = data.imageType;
                buttonImage.color = Color.white; // 画像のティント
            }
            else
            {
                buttonImage.color = data.backgroundColor;
            }
        }
        
        // ホバーエフェクト設定
        if (data.enableHoverEffect)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = data.hoverColor;
            colors.pressedColor = data.pressedColor;
            colors.selectedColor = data.hoverColor;
            button.colors = colors;
        }
        
        // 枠線を追加
        if (data.showBorder)
        {
            AddBorder(buttonObj, data.borderColor, data.borderWidth);
        }
        
        // アイコンを追加
        if (data.iconSprite != null && data.iconPosition != IconPosition.None)
        {
            AddIcon(buttonObj, data);
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
    /// ボタンの位置を設定（自動配置モード）
    /// </summary>
    private void SetButtonPosition(RectTransform rectTransform, PageButtonData data, int index)
    {
        Vector2 position = startPosition;
        
        switch (layoutMode)
        {
            case LayoutMode.Vertical:
                // 縦方向に配置
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                
                float verticalOffset = index * (data.buttonHeight + buttonSpacing);
                position.y -= verticalOffset;
                rectTransform.anchoredPosition = position;
                break;
                
            case LayoutMode.Horizontal:
                // 横方向に配置
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(0f, 0.5f);
                rectTransform.pivot = new Vector2(0f, 0.5f);
                
                float horizontalOffset = index * (data.buttonWidth + buttonSpacing);
                position.x += horizontalOffset;
                rectTransform.anchoredPosition = position;
                break;
                
            case LayoutMode.Grid:
                // グリッド状に配置
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                
                int row = index / gridColumns;
                int col = index % gridColumns;
                
                float gridX = (col - (gridColumns - 1) / 2f) * gridCellSize.x;
                float gridY = -row * gridCellSize.y;
                
                position.x += gridX;
                position.y += gridY;
                rectTransform.anchoredPosition = position;
                break;
                
            case LayoutMode.Custom:
                // カスタムモードでもuseCustomPositionがfalseの場合は中央に配置
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                break;
        }
    }
    
    /// <summary>
    /// アンカープリセットを適用
    /// </summary>
    private void SetAnchorPreset(RectTransform rectTransform, AnchorPreset preset)
    {
        switch (preset)
        {
            case AnchorPreset.TopLeft:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                break;
            case AnchorPreset.TopCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                rectTransform.pivot = new Vector2(0.5f, 1);
                break;
            case AnchorPreset.TopRight:
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);
                break;
            case AnchorPreset.MiddleLeft:
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
                rectTransform.pivot = new Vector2(0, 0.5f);
                break;
            case AnchorPreset.Center:
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            case AnchorPreset.MiddleRight:
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.pivot = new Vector2(1, 0.5f);
                break;
            case AnchorPreset.BottomLeft:
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 0);
                break;
            case AnchorPreset.BottomCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);
                rectTransform.pivot = new Vector2(0.5f, 0);
                break;
            case AnchorPreset.BottomRight:
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                rectTransform.pivot = new Vector2(1, 0);
                break;
            case AnchorPreset.StretchTop:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(0.5f, 1);
                break;
            case AnchorPreset.StretchMiddle:
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            case AnchorPreset.StretchBottom:
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                rectTransform.pivot = new Vector2(0.5f, 0);
                break;
            case AnchorPreset.StretchLeft:
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 0.5f);
                break;
            case AnchorPreset.StretchCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            case AnchorPreset.StretchRight:
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 0.5f);
                break;
            case AnchorPreset.StretchAll:
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
        }
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
        
        // シーンの存在チェック
        if (!SceneExists(sceneName))
        {
            Debug.LogError($"シーン '{sceneName}' が見つかりません。以下を確認してください:\n" +
                          $"1. シーン名が正しいか（大文字小文字を含む）\n" +
                          $"2. シーンがビルド設定に追加されているか（File > Build Settings）\n" +
                          $"3. シーンファイルが存在するか");
            
#if UNITY_EDITOR
            // エディタでは詳細情報を表示
            bool foundInProject = UnityEditor.AssetDatabase.FindAssets($"t:Scene {sceneName}").Length > 0;
            if (foundInProject)
            {
                Debug.LogWarning($"シーン '{sceneName}' はプロジェクト内に存在しますが、ビルド設定に追加されていません。\n" +
                               $"File > Build Settings を開き、シーンを追加してください。");
            }
#endif
            return;
        }
        
        Debug.Log($"シーン '{sceneName}' に遷移します...");
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// シーンが存在するかチェック
    /// </summary>
    private bool SceneExists(string sceneName)
    {
        // ビルド設定内のシーンをチェック
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// アイコンをボタンに追加
    /// </summary>
    private void AddIcon(GameObject buttonObj, PageButtonData data)
    {
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(buttonObj.transform, false);
        
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = data.iconSprite;
        iconImage.preserveAspect = true;
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(data.iconSize, data.iconSize);
        
        // テキストオブジェクトを取得
        TextMeshProUGUI textMesh = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        RectTransform textRect = textMesh?.GetComponent<RectTransform>();
        
        // アイコンの位置設定
        switch (data.iconPosition)
        {
            case IconPosition.Left:
                iconRect.anchorMin = new Vector2(0, 0.5f);
                iconRect.anchorMax = new Vector2(0, 0.5f);
                iconRect.anchoredPosition = new Vector2(data.iconSize / 2 + 10, 0);
                if (textRect != null)
                {
                    textRect.offsetMin = new Vector2(data.iconSize + 20, textRect.offsetMin.y);
                }
                break;
                
            case IconPosition.Right:
                iconRect.anchorMin = new Vector2(1, 0.5f);
                iconRect.anchorMax = new Vector2(1, 0.5f);
                iconRect.anchoredPosition = new Vector2(-(data.iconSize / 2 + 10), 0);
                if (textRect != null)
                {
                    textRect.offsetMax = new Vector2(-(data.iconSize + 20), textRect.offsetMax.y);
                }
                break;
                
            case IconPosition.Top:
                iconRect.anchorMin = new Vector2(0.5f, 1);
                iconRect.anchorMax = new Vector2(0.5f, 1);
                iconRect.anchoredPosition = new Vector2(0, -(data.iconSize / 2 + 5));
                if (textRect != null)
                {
                    textRect.offsetMax = new Vector2(textRect.offsetMax.x, -(data.iconSize + 10));
                }
                break;
                
            case IconPosition.Bottom:
                iconRect.anchorMin = new Vector2(0.5f, 0);
                iconRect.anchorMax = new Vector2(0.5f, 0);
                iconRect.anchoredPosition = new Vector2(0, data.iconSize / 2 + 5);
                if (textRect != null)
                {
                    textRect.offsetMin = new Vector2(textRect.offsetMin.x, data.iconSize + 10);
                }
                break;
        }
    }
    
    /// <summary>
    /// 枠線をボタンに追加
    /// </summary>
    private void AddBorder(GameObject buttonObj, Color borderColor, float borderWidth)
    {
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(buttonObj.transform, false);
        
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = borderColor;
        
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        
        // Outlineコンポーネントで枠線を表現
        Outline outline = buttonObj.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(borderWidth, -borderWidth);
        
        // 枠線を背面に配置
        borderObj.transform.SetAsFirstSibling();
    }
    
    /// <summary>
    /// ボタンを再生成
    /// </summary>
    [ContextMenu("ボタンを再生成")]
    public void RegenerateButtons()
    {
        CreateButtons();
        Debug.Log("ボタンを再生成しました。");
    }
    
    /// <summary>
    /// ビルド設定内のシーン一覧をログ出力
    /// </summary>
    [ContextMenu("ビルド設定のシーン一覧を表示")]
    public void LogBuildScenes()
    {
        Debug.Log("=== ビルド設定内のシーン ===");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"[{i}] {sceneName} ({scenePath})");
        }
        
        if (SceneManager.sceneCountInBuildSettings == 0)
        {
            Debug.LogWarning("ビルド設定にシーンが追加されていません。\nFile > Build Settings を開き、シーンを追加してください。");
        }
    }
}
