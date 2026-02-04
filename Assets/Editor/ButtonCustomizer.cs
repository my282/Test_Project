using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ButtonCustomizer : EditorWindow
{
    private Color buttonColor = new Color(0.4f, 0.6f, 0.9f, 1f);
    private Color textColor = Color.white;
    private int fontSize = 24;
    private Vector2 buttonSize = new Vector2(250, 60);
    private float buttonSpacing = 25f;

    [MenuItem("Tools/ボタンをカスタマイズ")]
    public static void ShowWindow()
    {
        GetWindow<ButtonCustomizer>("ボタンカスタマイザー");
    }

    private void OnGUI()
    {
        GUILayout.Label("ボタンのデザイン設定", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        buttonColor = EditorGUILayout.ColorField("ボタンの色", buttonColor);
        textColor = EditorGUILayout.ColorField("テキストの色", textColor);
        fontSize = EditorGUILayout.IntSlider("フォントサイズ", fontSize, 10, 50);
        
        EditorGUILayout.Space();
        GUILayout.Label("ボタンの配置設定", EditorStyles.boldLabel);
        buttonSize = EditorGUILayout.Vector2Field("ボタンサイズ (幅, 高さ)", buttonSize);
        buttonSpacing = EditorGUILayout.Slider("ボタンの間隔", buttonSpacing, 5f, 100f);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("選択したボタンに設定を適用します", MessageType.Info);

        if (GUILayout.Button("選択したボタンに適用", GUILayout.Height(30)))
        {
            ApplyToSelectedButtons();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("全てのボタンに適用", GUILayout.Height(30)))
        {
            ApplyToAllButtons();
        }

        EditorGUILayout.Space();
        GUILayout.Label("プリセット", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("青系"))
        {
            buttonColor = new Color(0.4f, 0.6f, 0.9f, 1f);
            textColor = Color.white;
        }
        if (GUILayout.Button("緑系"))
        {
            buttonColor = new Color(0.4f, 0.8f, 0.5f, 1f);
            textColor = Color.white;
        }
        if (GUILayout.Button("赤系"))
        {
            buttonColor = new Color(0.9f, 0.4f, 0.4f, 1f);
            textColor = Color.white;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("オレンジ系"))
        {
            buttonColor = new Color(1f, 0.6f, 0.2f, 1f);
            textColor = Color.white;
        }
        if (GUILayout.Button("紫系"))
        {
            buttonColor = new Color(0.7f, 0.4f, 0.9f, 1f);
            textColor = Color.white;
        }
        if (GUILayout.Button("グレー"))
        {
            buttonColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            textColor = Color.black;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ApplyToSelectedButtons()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            ApplyToButton(obj);
        }
    }

    private void ApplyToAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button button in allButtons)
        {
            ApplyToButton(button.gameObject);
        }
    }

    private void ApplyToButton(GameObject buttonObj)
    {
        Button button = buttonObj.GetComponent<Button>();
        if (button == null) return;

        // ボタンのサイズ変更
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = buttonSize;
        }

        // ボタンの色変更
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = buttonColor;

            // ホバー・押下時の色も設定
            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = new Color(
                Mathf.Min(buttonColor.r * 1.2f, 1f),
                Mathf.Min(buttonColor.g * 1.2f, 1f),
                Mathf.Min(buttonColor.b * 1.2f, 1f),
                buttonColor.a
            );
            colors.pressedColor = new Color(
                buttonColor.r * 0.8f,
                buttonColor.g * 0.8f,
                buttonColor.b * 0.8f,
                buttonColor.a
            );
            button.colors = colors;
        }

        // テキストの色とサイズ変更
        Text text = buttonObj.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.color = textColor;
            text.fontSize = fontSize;
        }

        // ボタン間隔の適用（親にLayoutGroupがある場合）
        if (rectTransform != null && rectTransform.parent != null)
        {
            VerticalLayoutGroup layout = rectTransform.parent.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = buttonSpacing;
            }
        }

        EditorUtility.SetDirty(buttonObj);
        Debug.Log($"ボタン '{buttonObj.name}' のデザインを変更しました");
    }
}
