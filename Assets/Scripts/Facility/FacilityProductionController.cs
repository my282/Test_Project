using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Facilityの自動生成を管理するコントローラー
/// 各Facilityの生成処理を一元管理し、GameDatabaseと連携
/// シーンを跨いで永続化されるシングルトン
/// </summary>
public class FacilityProductionController : MonoBehaviour
{
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示")]
    [SerializeField] private bool showDebugLog = true;

    // シングルトンインスタンス
    private static FacilityProductionController instance;
    public static FacilityProductionController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FacilityProductionController>();
            }
            return instance;
        }
    }

    // 管理しているFacilityのリスト
    private List<Facility> managedFacilities = new List<Facility>();

    // TimerManagerが利用可能かどうか
    private bool useTimerManager = false;

    private void Awake()
    {
        // シングルトンパターン
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // シーンを跨いで永続化

        if (showDebugLog)
        {
            Debug.Log("FacilityProductionController: DontDestroyOnLoadに設定されました");
        }
    }

    private void Start()
    {
        // TimerManagerのイベントに登録
        if (TimerManager.Instance != null)
        {
            useTimerManager = true;
            TimerManager.Instance.OnTimerPausedChanged += OnTimerPausedChanged;
            TimerManager.Instance.OnTimerFinished += OnTimerFinished;
            TimerManager.Instance.OnTimerStarted += OnTimerStarted;

            if (showDebugLog)
            {
                Debug.Log("FacilityProductionController: TimerManagerと統合しました");
            }
        }
        else
        {
            Debug.LogWarning("FacilityProductionController: TimerManagerが見つかりません。Time.timeをフォールバックとして使用します。");
        }
    }

    private void OnDestroy()
    {
        // イベント解除
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTimerPausedChanged -= OnTimerPausedChanged;
            TimerManager.Instance.OnTimerFinished -= OnTimerFinished;
            TimerManager.Instance.OnTimerStarted -= OnTimerStarted;
        }
    }

    private void Update()
    {
        // TimerManagerが利用可能な場合はそちらを使用
        if (useTimerManager && TimerManager.Instance != null && TimerManager.Instance.IsRunning)
        {
            UpdateFacilityProductionWithTimer();
        }
        else if (!useTimerManager || TimerManager.Instance == null)
        {
            // フォールバック: Time.timeを使用
            UpdateFacilityProduction();
        }
        // else: TimerManagerはあるが停止中 → 何もしない
    }

    /// <summary>
    /// Facilityを管理対象に追加
    /// </summary>
    public void RegisterFacility(Facility facility)
    {
        if (facility == null)
        {
            Debug.LogWarning("Null facility cannot be registered.");
            return;
        }

        if (managedFacilities.Contains(facility))
        {
            Debug.LogWarning($"Facility {facility.facilityName} is already registered.");
            return;
        }

        managedFacilities.Add(facility);
        
        // TimerManagerがあればそちらを使用、なければTime.time
        if (useTimerManager && TimerManager.Instance != null)
        {
            facility.productionState.lastProductionTime = TimerManager.Instance.CurrentTime;
        }
        else
        {
            facility.productionState.lastProductionTime = Time.time;
        }

        if (showDebugLog)
        {
            Debug.Log($"Facility '{facility.facilityName}' registered for production.");
            Debug.Log($"  - Production Type: {facility.productionConfig.productionType}");
            Debug.Log($"  - Interval: {facility.productionConfig.productionInterval}s");
            Debug.Log($"  - Using Timer: {(useTimerManager ? "TimerManager" : "Time.time")}");
            Debug.Log($"  - IsValid: {facility.productionConfig.IsValid()}");
        }
    }

    /// <summary>
    /// Facilityを管理対象から削除
    /// </summary>
    public void UnregisterFacility(Facility facility)
    {
        if (managedFacilities.Contains(facility))
        {
            managedFacilities.Remove(facility);

            if (showDebugLog)
            {
                Debug.Log($"Facility '{facility.facilityName}' unregistered from production.");
            }
        }
    }

    /// <summary>
    /// すべてのFacilityの生成状態を更新（Time.timeを使用したフォールバック）
    /// </summary>
    private void UpdateFacilityProduction()
    {
        if (managedFacilities.Count == 0) return;
        
        foreach (var facility in managedFacilities)
        {
            // 解放されていない、または設定が無効な場合はスキップ
            if (!facility.isUnlocked || !facility.productionConfig.IsValid())
                continue;

            // 一時停止中はスキップ
            if (facility.productionState.isPaused)
                continue;

            // 時間チェック（Time.timeを使用）
            float currentTime = Time.time;
            float elapsed = currentTime - facility.productionState.lastProductionTime;

            if (elapsed >= facility.productionConfig.productionInterval)
            {
                ProduceFacilityResources(facility);
                facility.productionState.lastProductionTime = currentTime;
            }
        }
    }

    /// <summary>
    /// TimerManagerを使用した生成更新
    /// </summary>
    private void UpdateFacilityProductionWithTimer()
    {
        if (managedFacilities.Count == 0) return;
        
        foreach (var facility in managedFacilities)
        {
            // 解放されていない、または設定が無効な場合はスキップ
            if (!facility.isUnlocked || !facility.productionConfig.IsValid())
                continue;

            // 一時停止中はスキップ
            if (facility.productionState.isPaused)
                continue;

            // TimerManagerのCheckIntervalメソッドを使用
            if (TimerManager.Instance.CheckInterval(
                facility.productionConfig.productionInterval,
                ref facility.productionState.lastProductionTime))
            {
                ProduceFacilityResources(facility);
            }
        }
    }

    /// <summary>
    /// Facilityのリソースを生成
    /// </summary>
    private void ProduceFacilityResources(Facility facility)
    {
        int moneyProduced = 0;
        Dictionary<string, int> itemsProduced = new Dictionary<string, int>();

        // お金の生成
        if (facility.productionConfig.ProducesMoney())
        {
            // レベルに基づく倍率を適用
            float multiplier = facility.GetProductionMultiplier();
            moneyProduced = Mathf.RoundToInt(facility.productionConfig.moneyAmount * multiplier);
            
            // GameDatabaseにお金を追加
            if (GameDatabase.Instance != null)
            {
                GameDatabase.Instance.AddMoney(moneyProduced);
            }
            else
            {
                Debug.LogWarning($"[{facility.facilityName}] GameDatabaseが見つかりません。お金を追加できません。");
            }
            
            if (showDebugLog)
            {
                Debug.Log($"[{facility.facilityName}] お金を生成: {moneyProduced} (Lv{facility.level}, x{multiplier:F1})");
            }
        }

        // アイテムの生成
        if (facility.productionConfig.ProducesItems())
        {
            // レベルに基づく倍率を適用
            float multiplier = facility.GetProductionMultiplier();
            
            foreach (var itemProd in facility.productionConfig.itemProductions)
            {
                string itemId = itemProd.GetItemId();
                int quantity = Mathf.RoundToInt(itemProd.quantity * multiplier);

                // GameDatabaseにアイテムを追加
                if (GameDatabase.Instance != null)
                {
                    // MasterDatabaseからItemDataを取得
                    ItemData itemData = MasterDatabase.Instance?.GetItemData(itemId);
                    
                    if (itemData != null)
                    {
                        GameDatabase.Instance.AddItem(
                            itemId, 
                            itemData.itemName, 
                            itemData.description, 
                            quantity, 
                            itemData.type,
                            itemData.icon
                        );
                    }
                    else
                    {
                        Debug.LogWarning($"[{facility.facilityName}] ItemData '{itemId}' が見つかりません。MasterDatabaseに登録してください。");
                    }
                }
                else
                {
                    Debug.LogWarning($"[{facility.facilityName}] GameDatabaseが見つかりません。アイテムを追加できません。");
                }

                itemsProduced[itemId] = quantity;

                if (showDebugLog)
                {
                    Debug.Log($"[{facility.facilityName}] アイテムを生成: {itemId} x{quantity} (Lv{facility.level}, x{multiplier:F1})");
                }
            }
        }

        // 生成記録
        facility.productionState.RecordProduction(moneyProduced, itemsProduced);

        if (showDebugLog)
        {
            Debug.Log($"[{facility.facilityName}] 生成完了 (累計: {facility.productionState.totalProductionCount}回)");
        }
    }

    /// <summary>
    /// 特定のFacilityの生成を一時停止/再開
    /// </summary>
    public void SetFacilityProductionPaused(string facilityId, bool paused)
    {
        Facility facility = managedFacilities.Find(f => f.facilityId == facilityId);
        if (facility != null)
        {
            facility.productionState.isPaused = paused;

            if (showDebugLog)
            {
                Debug.Log($"Facility '{facility.facilityName}' production {(paused ? "paused" : "resumed")}.");
            }
        }
    }

    /// <summary>
    /// すべてのFacilityの生成を一時停止/再開
    /// </summary>
    public void SetAllProductionPaused(bool paused)
    {
        foreach (var facility in managedFacilities)
        {
            facility.productionState.isPaused = paused;
        }

        if (showDebugLog)
        {
            Debug.Log($"All facility production {(paused ? "paused" : "resumed")}.");
        }
    }

    /// <summary>
    /// 特定のFacilityを手動で生成実行
    /// </summary>
    public void ManualProduceResources(string facilityId)
    {
        Facility facility = managedFacilities.Find(f => f.facilityId == facilityId);
        if (facility != null && facility.isUnlocked)
        {
            ProduceFacilityResources(facility);
            facility.productionState.lastProductionTime = Time.time;
        }
    }

    /// <summary>
    /// 特定のFacilityの統計情報を取得
    /// </summary>
    public ProductionState GetFacilityProductionState(string facilityId)
    {
        Facility facility = managedFacilities.Find(f => f.facilityId == facilityId);
        return facility?.productionState;
    }

    /// <summary>
    /// すべてのFacilityの統計情報をログ出力
    /// </summary>
    [ContextMenu("Show All Production Stats")]
    public void ShowAllProductionStats()
    {
        Debug.Log("=== Facility Production Statistics ===");
        foreach (var facility in managedFacilities)
        {
            Debug.Log($"{facility.facilityName}:");
            Debug.Log($"  生成回数: {facility.productionState.totalProductionCount}");
            Debug.Log($"  総お金生成: {facility.productionState.totalMoneyProduced}");
            
            foreach (var item in facility.productionState.totalItemsProduced)
            {
                Debug.Log($"  {item.Key}: {item.Value}個");
            }
        }
    }

    /// <summary>
    /// すべてのFacilityの生成状態をリセット
    /// </summary>
    public void ResetAllProductionStates()
    {
        foreach (var facility in managedFacilities)
        {
            facility.ResetProductionState();
        }

        if (showDebugLog)
        {
            Debug.Log("All facility production states reset.");
        }
    }

    // ========================================
    // TimerManagerイベントハンドラー
    // ========================================

    /// <summary>
    /// タイマー開始時の処理
    /// </summary>
    private void OnTimerStarted()
    {
        if (showDebugLog)
        {
            Debug.Log("FacilityProductionController: タイマーが開始されました。生成を開始します。");
        }

        // すべてのFacilityの一時停止を解除
        SetAllProductionPaused(false);

        // lastProductionTimeをTimerManagerの時間で初期化
        foreach (var facility in managedFacilities)
        {
            facility.productionState.lastProductionTime = TimerManager.Instance.CurrentTime;
        }
    }

    /// <summary>
    /// タイマー一時停止状態変更時の処理
    /// </summary>
    private void OnTimerPausedChanged(bool isPaused)
    {
        if (showDebugLog)
        {
            Debug.Log($"FacilityProductionController: タイマーが{(isPaused ? "一時停止" : "再開")}されました。");
        }

        // すべてのFacilityの生成を一時停止/再開
        SetAllProductionPaused(isPaused);
    }

    /// <summary>
    /// タイマー終了時の処理
    /// </summary>
    private void OnTimerFinished()
    {
        if (showDebugLog)
        {
            Debug.Log("FacilityProductionController: タイマーが終了しました。生成を停止します。");
            ShowAllProductionStats();
        }

        // すべての生成を停止
        SetAllProductionPaused(true);
    }
}
