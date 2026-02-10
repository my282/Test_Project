using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// シーン遷移をサポートするユーティリティクラス
/// </summary>
public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;
    
    [Header("フェード設定")]
    [Tooltip("シーン遷移時にフェードエフェクトを使用")]
    public bool useFadeEffect = false;
    
    [Tooltip("フェード時間（秒）")]
    public float fadeDuration = 0.5f;
    
    private void Awake()
    {
        // シングルトンパターン
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// シーンを読み込む（静的メソッド）
    /// </summary>
    public static void Load(string sceneName)
    {
        if (instance != null && instance.useFadeEffect)
        {
            instance.StartCoroutine(instance.LoadSceneWithFade(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// シーンをインデックスで読み込む
    /// </summary>
    public static void LoadByIndex(int sceneIndex)
    {
        if (instance != null && instance.useFadeEffect)
        {
            instance.StartCoroutine(instance.LoadSceneWithFadeByIndex(sceneIndex));
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
    
    /// <summary>
    /// ハブページに戻る
    /// </summary>
    public static void ReturnToHub()
    {
        Load("hub_page");
    }
    
    /// <summary>
    /// 前のシーンに戻る
    /// </summary>
    public static void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex > 0)
        {
            LoadByIndex(currentSceneIndex - 1);
        }
        else
        {
            Debug.LogWarning("前のシーンがありません。");
        }
    }
    
    /// <summary>
    /// 次のシーンに進む
    /// </summary>
    public static void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        
        if (currentSceneIndex < totalScenes - 1)
        {
            LoadByIndex(currentSceneIndex + 1);
        }
        else
        {
            Debug.LogWarning("次のシーンがありません。");
        }
    }
    
    /// <summary>
    /// フェード付きでシーンを読み込む
    /// </summary>
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        // フェードアウト処理をここに追加可能
        yield return new WaitForSeconds(fadeDuration);
        
        SceneManager.LoadScene(sceneName);
        
        // フェードイン処理をここに追加可能
    }
    
    /// <summary>
    /// フェード付きでシーンを読み込む（インデックス版）
    /// </summary>
    private IEnumerator LoadSceneWithFadeByIndex(int sceneIndex)
    {
        // フェードアウト処理をここに追加可能
        yield return new WaitForSeconds(fadeDuration);
        
        SceneManager.LoadScene(sceneIndex);
        
        // フェードイン処理をここに追加可能
    }
    
    /// <summary>
    /// ゲームを終了
    /// </summary>
    public static void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
