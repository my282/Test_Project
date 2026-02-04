using UnityEngine;
using UnityEngine.EventSystems;

// 画像、テキスト、パネルなど、どんなUI要素でもクリックで画面遷移できるようにする
public class ClickableScreen : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ScreenType
    {
        MainMenu,
        Game,
        Settings
    }

    [Header("遷移先の画面タイプ")]
    public ScreenType targetScreen;

    [Header("シーン遷移の場合はシーン名を指定")]
    public string targetSceneName;

    [Header("遷移タイプ")]
    public bool useSceneTransition = false;

    [Header("ホバーエフェクト")]
    public bool enableHoverEffect = true;
    public float hoverScale = 1.1f;
    public Color hoverTint = new Color(1f, 1f, 1f, 0.8f);

    private Vector3 originalScale;
    private Color originalColor;
    private UnityEngine.UI.Graphic graphic;

    private void Awake()
    {
        originalScale = transform.localScale;
        
        // Image、Text、RawImageなどのGraphicコンポーネントを取得
        graphic = GetComponent<UnityEngine.UI.Graphic>();
        if (graphic != null)
        {
            originalColor = graphic.color;
        }
    }

    // クリック時
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"クリックされました: {gameObject.name}");

        if (ScreenManager.Instance == null)
        {
            Debug.LogError("ScreenManager instance not found!");
            return;
        }

        if (useSceneTransition && !string.IsNullOrEmpty(targetSceneName))
        {
            // シーン遷移
            Debug.Log($"シーン遷移: {targetSceneName}");
            ScreenManager.Instance.LoadScene(targetSceneName);
        }
        else
        {
            // パネル切り替え
            Debug.Log($"パネル切り替え: {targetScreen}");
            switch (targetScreen)
            {
                case ScreenType.MainMenu:
                    ScreenManager.Instance.ShowMainMenu();
                    break;
                case ScreenType.Game:
                    ScreenManager.Instance.ShowGame();
                    break;
                case ScreenType.Settings:
                    ScreenManager.Instance.ShowSettings();
                    break;
            }
        }
    }

    // マウスカーソルが上に来た時
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableHoverEffect) return;

        transform.localScale = originalScale * hoverScale;
        
        if (graphic != null)
        {
            graphic.color = hoverTint;
        }
    }

    // マウスカーソルが離れた時
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enableHoverEffect) return;

        transform.localScale = originalScale;
        
        if (graphic != null)
        {
            graphic.color = originalColor;
        }
    }
}
