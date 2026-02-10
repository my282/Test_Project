using UnityEngine;

[System.Serializable]
public class PageButtonData
{
    [Header("ページ設定")]
    [Tooltip("遷移先のシーン名")]
    public string sceneName;
    
    [Tooltip("ボタンに表示するテキスト")]
    public string buttonText;
    
    [Header("ボタンデザイン")]
    [Tooltip("ボタンの背景色")]
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
}
