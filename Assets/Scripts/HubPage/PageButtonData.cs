using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PageButtonData
{
    [Header("ページ設定")]
    [Tooltip("遷移先のシーン名")]
    public string sceneName;
    
    [Tooltip("ボタンに表示するテキスト")]
    public string buttonText;
    
    [Header("位置設定")]
    [Tooltip("カスタム位置を使用（チェックを外すと自動配置）")]
    public bool useCustomPosition = false;
    
    [Tooltip("カスタム位置（X, Y）")]
    public Vector2 customPosition = Vector2.zero;
    
    [Tooltip("アンカー設定")]
    public AnchorPreset anchorPreset = AnchorPreset.Center;
    
    [Header("ボタン画像設定")]
    [Tooltip("ボタンの背景画像（Sprite）。設定した場合は背景色よりも優先されます")]
    public Sprite backgroundSprite;
    
    [Tooltip("画像のタイプ（Simple, Sliced, Tiled, Filled）")]
    public Image.Type imageType = Image.Type.Simple;
    
    [Tooltip("ボタンのアイコン画像（オプション）")]
    public Sprite iconSprite;
    
    [Tooltip("アイコンのサイズ")]
    [Range(20, 100)]
    public float iconSize = 40f;
    
    [Tooltip("アイコンの位置（左から）")]
    public IconPosition iconPosition = IconPosition.Left;
    
    [Header("ボタンデザイン")]
    [Tooltip("ボタンの背景色（画像が設定されていない場合に使用）")]
    public Color backgroundColor = Color.white;
    
    [Tooltip("ボタンのテキスト色")]
    public Color textColor = Color.black;
    
    [Tooltip("ボタンのフォントサイズ")]
    [Range(10, 72)]
    public int fontSize = 24;
    
    [Tooltip("ボタンの幅")]
    [Range(100, 600)]
    public float buttonWidth = 300f;
    
    [Tooltip("ボタンの高さ")]
    [Range(30, 200)]
    public float buttonHeight = 60f;
    
    [Header("ホバーエフェクト")]
    [Tooltip("ホバー時の色変更を有効化")]
    public bool enableHoverEffect = true;
    
    [Tooltip("ホバー時の色")]
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    
    [Tooltip("クリック時の色")]
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    [Header("枠線設定")]
    [Tooltip("枠線を表示")]
    public bool showBorder = false;
    
    [Tooltip("枠線の色")]
    public Color borderColor = Color.black;
    
    [Tooltip("枠線の太さ")]
    [Range(1, 10)]
    public float borderWidth = 2f;
}

public enum IconPosition
{
    None,       // アイコンなし
    Left,       // 左側
    Right,      // 右側
    Top,        // 上側
    Bottom      // 下側
}

public enum AnchorPreset
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    Center,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
    StretchTop,
    StretchMiddle,
    StretchBottom,
    StretchLeft,
    StretchCenter,
    StretchRight,
    StretchAll
}
