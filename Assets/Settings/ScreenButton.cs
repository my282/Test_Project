using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ScreenButton : MonoBehaviour
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

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        Debug.Log($"ボタンがクリックされました: {gameObject.name}");

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
}
