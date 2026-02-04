using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ImageButtonCreator : EditorWindow
{
    private Sprite selectedSprite;
    private ClickableScreen.ScreenType targetScreen = ClickableScreen.ScreenType.Game;
    private Vector2 buttonSize = new Vector2(300, 100);
    private bool addText = true;
    private string buttonText = "クリックしてね";
    private bool enableHover = true;
    private float hoverScale = 1.1f;

    [MenuItem("Tools/画像からボタンを作成")]
    public static void ShowWindow()
    {
        GetWindow<ImageButtonCreator>("画像ボタン作成");
    }

    private void OnGUI()
    {
        GUILayout.Label("画像からボタンを作成", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("1. 画像をAssetsフォルダにドラッグ＆ドロップ\n2. 下のフィールドに画像を設定\n3. 「作成」ボタンをクリック", MessageType.Info);
        EditorGUILayout.Space();

        selectedSprite = (Sprite)EditorGUILayout.ObjectField("使用する画像", selectedSprite, typeof(Sprite), false);

        EditorGUILayout.Space();
        GUILayout.Label("ボタン設定", EditorStyles.boldLabel);

        targetScreen = (ClickableScreen.ScreenType)EditorGUILayout.EnumPopup("遷移先画面", targetScreen);
        buttonSize = EditorGUILayout.Vector2Field("ボタンサイズ", buttonSize);

        EditorGUILayout.Space();
        addText = EditorGUILayout.Toggle("テキストを追加", addText);
        if (addText)
        {
            EditorGUI.indentLevel++;
            buttonText = EditorGUILayout.TextField("テキスト内容", buttonText);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        enableHover = EditorGUILayout.Toggle("ホバーエフェクト", enableHover);
        if (enableHover)
        {
            EditorGUI.indentLevel++;
            hoverScale = EditorGUILayout.Slider("拡大率", hoverScale, 1.0f, 2.0f);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        GUI.enabled = selectedSprite != null;
        if (GUILayout.Button("画像ボタンを作成", GUILayout.Height(40)))
        {
            CreateImageButton();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("作成後、SceneビューまたはCanvas内にドラッグして配置してください", MessageType.Info);
    }

    private void CreateImageButton()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("エラー", "Canvasが見つかりません！\n先に「画面遷移システムをセットアップ」を実行してください。", "OK");
            return;
        }

        // 画像ボタンを作成
        GameObject buttonObj = new GameObject(selectedSprite.name + "_Button");
        buttonObj.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = buttonSize;
        rectTransform.anchoredPosition = Vector2.zero;

        // 画像を設定
        Image image = buttonObj.AddComponent<Image>();
        image.sprite = selectedSprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;

        // クリック可能にする
        ClickableScreen clickable = buttonObj.AddComponent<ClickableScreen>();
        clickable.targetScreen = targetScreen;
        clickable.enableHoverEffect = enableHover;
        clickable.hoverScale = hoverScale;

        // テキストを追加
        if (addText)
        {
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
            text.color = Color.white;

            // 影を追加して読みやすくする
            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
        }

        Debug.Log($"画像ボタン '{buttonObj.name}' を作成しました！");
        Selection.activeGameObject = buttonObj;
        EditorUtility.DisplayDialog("完了", $"画像ボタン '{buttonObj.name}' を作成しました！\n\nCanvas内の好きな場所にドラッグして配置してください。", "OK");
    }
}
