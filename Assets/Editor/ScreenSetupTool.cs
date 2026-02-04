using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

public class ScreenSetupTool : EditorWindow
{
    [MenuItem("Tools/画面遷移システムをセットアップ")]
    public static void SetupScreenSystem()
    {
        // ScreenManagerオブジェクトを作成
        GameObject screenManagerObj = new GameObject("ScreenManager");
        ScreenManager screenManager = screenManagerObj.AddComponent<ScreenManager>();

        // Canvasを作成（既に存在する場合はそれを使用）
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // EventSystemを作成（既に存在する場合はスキップ）
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 3つのパネルを作成（エディタでは横並び）
        GameObject mainMenuPanel = CreatePanel(canvas.transform, "MainMenuPanel", new Color(0.2f, 0.3f, 0.5f, 0.9f), 0);
        GameObject gamePanel = CreatePanel(canvas.transform, "GamePanel", new Color(0.3f, 0.5f, 0.3f, 0.9f), 1);
        GameObject settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", new Color(0.5f, 0.3f, 0.3f, 0.9f), 2);

        // ScreenManagerにパネルを設定
        screenManager.mainMenuPanel = mainMenuPanel;
        screenManager.gamePanel = gamePanel;
        screenManager.settingsPanel = settingsPanel;

        // 各パネルにボタンを追加
        CreateButtonInPanel(mainMenuPanel.transform, "ゲーム開始", ScreenButton.ScreenType.Game);
        CreateButtonInPanel(mainMenuPanel.transform, "設定", ScreenButton.ScreenType.Settings);

        CreateButtonInPanel(gamePanel.transform, "メインメニューに戻る", ScreenButton.ScreenType.MainMenu);

        CreateButtonInPanel(settingsPanel.transform, "戻る", ScreenButton.ScreenType.MainMenu);

        Debug.Log("画面遷移システムのセットアップが完了しました！");
        Selection.activeGameObject = screenManagerObj;
    }

    [MenuItem("Tools/クリック可能な画像を追加")]
    public static void CreateClickableImage()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvasが見つかりません！先に画面遷移システムをセットアップしてください。");
            return;
        }

        // クリック可能な画像を作成
        GameObject imageObj = new GameObject("ClickableImage");
        imageObj.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(200, 200);

        UnityEngine.UI.Image image = imageObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.8f, 0.5f, 0.9f, 1f);

        // ClickableScreenコンポーネントを追加
        ClickableScreen clickable = imageObj.AddComponent<ClickableScreen>();
        clickable.targetScreen = ClickableScreen.ScreenType.Game;

        // テキストラベルを追加
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(imageObj.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "クリックで遷移";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 20;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;

        Debug.Log("クリック可能な画像を作成しました！");
        Selection.activeGameObject = imageObj;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color, int index)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        
        // エディタでは横並びに配置（3分割）
        float width = 1f / 3f;
        rectTransform.anchorMin = new Vector2(width * index, 0);
        rectTransform.anchorMax = new Vector2(width * (index + 1), 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = color;

        // タイトルテキストを追加
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(400, 100);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = name;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 36;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        return panel;
    }

    private static void CreateButtonInPanel(Transform parent, string buttonText, ScreenButton.ScreenType targetScreen)
    {
        // ボタン配置用のコンテナ（なければ作成）
        Transform buttonContainer = parent.Find("ButtonContainer");
        if (buttonContainer == null)
        {
            GameObject containerObj = new GameObject("ButtonContainer");
            containerObj.transform.SetParent(parent, false);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.4f);
            containerRect.anchorMax = new Vector2(0.5f, 0.4f);
            containerRect.sizeDelta = new Vector2(200, 200);

            VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;

            buttonContainer = containerObj.transform;
        }

        // ボタンオブジェクトを作成
        GameObject buttonObj = new GameObject(buttonText + "Button");
        buttonObj.transform.SetParent(buttonContainer, false);

        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        button.colors = colors;

        // ScreenButtonコンポーネントをアタッチ
        ScreenButton screenButton = buttonObj.AddComponent<ScreenButton>();
        screenButton.targetScreen = targetScreen;

        // ボタンのテキスト
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
    }
}
