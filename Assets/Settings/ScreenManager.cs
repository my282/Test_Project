using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject settingsPanel;

    private void Awake()
    {
        // シングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // EventSystemが存在しない場合は自動で作成
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            
            // New Input Systemを使用している場合
            var inputModule = eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("EventSystem (Input System対応) を自動生成しました");
        }
    }

    private void Start()
    {
        // 実行時に全パネルを全画面にする
        SetupFullScreenPanels();
        
        // 最初はメインメニューを表示
        ShowMainMenu();
    }

    // 実行時に各パネルを全画面にセットアップ
    private void SetupFullScreenPanels()
    {
        SetPanelFullScreen(mainMenuPanel);
        SetPanelFullScreen(gamePanel);
        SetPanelFullScreen(settingsPanel);
    }

    private void SetPanelFullScreen(GameObject panel)
    {
        if (panel == null) return;

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }

    // メインメニューを表示
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }

    // ゲーム画面を表示
    public void ShowGame()
    {
        SetActivePanel(gamePanel);
    }

    // 設定画面を表示
    public void ShowSettings()
    {
        SetActivePanel(settingsPanel);
    }

    // 指定されたパネルのみをアクティブにする
    private void SetActivePanel(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == activePanel);
        if (gamePanel != null) gamePanel.SetActive(gamePanel == activePanel);
        if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == activePanel);
    }

    // シーン遷移（シーン名を指定）
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // アプリケーション終了
    public void QuitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
