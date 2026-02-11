using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// シーン間で永続化されるUI要素を管理するマネージャー
/// タイマー、総資産など、常に表示が必要な情報を統合管理
/// </summary>
public class PersistentUIManager : MonoBehaviour
{
    private static PersistentUIManager instance;
    public static PersistentUIManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PersistentUIManager");
                instance = go.AddComponent<PersistentUIManager>();
            }
            return instance;
        }
    }

    [Header("UI Canvas設定")]
    [Tooltip("永続UI用のCanvas（未設定時は自動生成）")]
    [SerializeField] private Canvas persistentCanvas;

    [Tooltip("Canvasの表示順序")]
    [SerializeField] private int canvasSortOrder = 100;

    [Header("UI要素参照")]
    [Tooltip("タイマー表示のGameObject")]
    [SerializeField] private GameObject timerDisplayObject;

    [Tooltip("情報パネルのコンテナ")]
    [SerializeField] private Transform infoContainer;

    [Header("タイマー表示位置設定")]
    [Tooltip("スクリプトでタイマー位置を設定する（falseの場合、Sceneビューで直接調整可能）")]
    [SerializeField] private bool overrideTimerPosition = false;

    [Tooltip("タイマーのアンカー最小値（0,0=左下、1,1=右上）")]
    [SerializeField] private Vector2 timerAnchorMin = new Vector2(0.5f, 1f); // 上部中央

    [Tooltip("タイマーのアンカー最大値（0,0=左下、1,1=右上）")]
    [SerializeField] private Vector2 timerAnchorMax = new Vector2(0.5f, 1f); // 上部中央

    [Tooltip("タイマーのピボット（回転/スケールの中心点）")]
    [SerializeField] private Vector2 timerPivot = new Vector2(0.5f, 1f);

    [Tooltip("タイマーの位置（アンカーからのオフセット）")]
    [SerializeField] private Vector2 timerPosition = new Vector2(0, -20); // 上から20px下

    // 追加の表示要素を管理するリスト（将来の拡張用）
    private Dictionary<string, GameObject> registeredDisplays = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // シングルトンパターン
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUI();
    }

    /// <summary>
    /// UI初期化
    /// </summary>
    private void InitializeUI()
    {
        // Canvasが設定されていない場合は作成
        if (persistentCanvas == null)
        {
            CreatePersistentCanvas();
        }
        else
        {
            DontDestroyOnLoad(persistentCanvas.gameObject);
        }

        // Canvasの設定
        persistentCanvas.sortingOrder = canvasSortOrder;

        // タイマー表示の初期化
        if (timerDisplayObject != null)
        {
            RectTransform timerRect = timerDisplayObject.GetComponent<RectTransform>();
            
            // スクリプトで位置を上書きする場合
            if (overrideTimerPosition)
            {
                timerDisplayObject.transform.SetParent(persistentCanvas.transform, false);
                
                if (timerRect != null)
                {
                    timerRect.anchorMin = timerAnchorMin;
                    timerRect.anchorMax = timerAnchorMax;
                    timerRect.pivot = timerPivot;
                    timerRect.anchoredPosition = timerPosition;
                }
            }
            else
            {
                // エディターで設定した位置を保持する場合
                if (timerRect != null)
                {
                    // 現在の位置情報を保存
                    Vector2 savedAnchorMin = timerRect.anchorMin;
                    Vector2 savedAnchorMax = timerRect.anchorMax;
                    Vector2 savedPivot = timerRect.pivot;
                    Vector2 savedPosition = timerRect.anchoredPosition;
                    
                    // 親を設定（worldPositionStays = trueで位置を保持しようとする）
                    timerDisplayObject.transform.SetParent(persistentCanvas.transform, true);
                    
                    // 念のため位置情報を復元
                    timerRect.anchorMin = savedAnchorMin;
                    timerRect.anchorMax = savedAnchorMax;
                    timerRect.pivot = savedPivot;
                    timerRect.anchoredPosition = savedPosition;
                }
                else
                {
                    timerDisplayObject.transform.SetParent(persistentCanvas.transform, true);
                }
            }
        }
    }

    /// <summary>
    /// 永続Canvasを作成
    /// </summary>
    private void CreatePersistentCanvas()
    {
        GameObject canvasObj = new GameObject("PersistentCanvas");
        persistentCanvas = canvasObj.AddComponent<Canvas>();
        persistentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        persistentCanvas.sortingOrder = canvasSortOrder;

        // CanvasScaler追加
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // GraphicRaycaster追加
        canvasObj.AddComponent<GraphicRaycaster>();

        DontDestroyOnLoad(canvasObj);

        // 情報コンテナを作成
        CreateInfoContainer();
    }

    /// <summary>
    /// 情報表示用のコンテナを作成
    /// </summary>
    private void CreateInfoContainer()
    {
        GameObject containerObj = new GameObject("InfoContainer");
        containerObj.transform.SetParent(persistentCanvas.transform, false);

        RectTransform rect = containerObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1); // 右上
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, -20); // 右上から少しオフセット

        // レイアウトグループ追加（縦に並べる）
        VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperRight;
        layout.spacing = 10;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        infoContainer = containerObj.transform;
    }

    /// <summary>
    /// 表示要素を登録（将来の拡張用）
    /// </summary>
    /// <param name="key">識別キー</param>
    /// <param name="displayObject">表示するGameObject</param>
    public void RegisterDisplay(string key, GameObject displayObject)
    {
        if (registeredDisplays.ContainsKey(key))
        {
            Debug.LogWarning($"Display with key '{key}' is already registered.");
            return;
        }

        registeredDisplays.Add(key, displayObject);
        
        if (infoContainer != null)
        {
            displayObject.transform.SetParent(infoContainer, false);
        }

        Debug.Log($"Display '{key}' registered.");
    }

    /// <summary>
    /// 表示要素の登録解除
    /// </summary>
    public void UnregisterDisplay(string key)
    {
        if (registeredDisplays.ContainsKey(key))
        {
            GameObject display = registeredDisplays[key];
            registeredDisplays.Remove(key);
            
            if (display != null)
            {
                Destroy(display);
            }

            Debug.Log($"Display '{key}' unregistered.");
        }
    }

    /// <summary>
    /// 登録された表示要素を取得
    /// </summary>
    public GameObject GetDisplay(string key)
    {
        if (registeredDisplays.ContainsKey(key))
        {
            return registeredDisplays[key];
        }
        return null;
    }

    /// <summary>
    /// すべての表示要素の表示/非表示を切り替え
    /// </summary>
    public void ToggleAllDisplays(bool visible)
    {
        if (persistentCanvas != null)
        {
            persistentCanvas.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 特定の表示要素の表示/非表示を切り替え
    /// </summary>
    public void ToggleDisplay(string key, bool visible)
    {
        if (registeredDisplays.ContainsKey(key))
        {
            registeredDisplays[key].SetActive(visible);
        }
    }

    /// <summary>
    /// タイマー表示の表示/非表示
    /// </summary>
    public void ToggleTimerDisplay(bool visible)
    {
        if (timerDisplayObject != null)
        {
            timerDisplayObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Canvasの表示順序を設定
    /// </summary>
    public void SetCanvasSortOrder(int order)
    {
        canvasSortOrder = order;
        if (persistentCanvas != null)
        {
            persistentCanvas.sortingOrder = order;
        }
    }
}
