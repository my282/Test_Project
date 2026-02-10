using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Facilityの自動生成を管理するコントローラー
/// 各Facilityの生成処理を一元管理し、GameDatabaseと連携
/// </summary>
public class FacilityProductionController : MonoBehaviour
{
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示")]
    [SerializeField] private bool showDebugLog = true;

    // 管理しているFacilityのリスト
    private List<Facility> managedFacilities = new List<Facility>();

    // TODO: タイマー機能実装後の統合
    // ========================================
    // TimerManagerと連携する際の実装手順:
    // 
    // 1. TimerManager.Instanceの存在チェック
    //    if (TimerManager.Instance == null) return;
    //
    // 2. TimerManager.Instance.CheckInterval()を使用
    //    foreach (var facility in managedFacilities)
    //    {
    //        if (TimerManager.Instance.CheckInterval(
    //            facility.productionConfig.productionInterval,
    //            ref facility.productionState.lastProductionTime))
    //        {
    //            ProduceFacilityResources(facility);
    //        }
    //    }
    //
    // 3. TimerManagerのイベントに登録
    //    void Start()
    //    {
    //        TimerManager.Instance.OnTimerPausedChanged += OnTimerPausedChanged;
    //        TimerManager.Instance.OnTimerFinished += OnTimerFinished;
    //    }
    //
    // 4. イベントハンドラーの実装
    //    void OnTimerPausedChanged(bool isPaused)
    //    {
    //        foreach (var facility in managedFacilities)
    //        {
    //            facility.productionState.isPaused = isPaused;
    //        }
    //    }
    // ========================================

    private void Update()
    {
        // 現在は通常のTime.timeを使用（タイマー実装後に置き換え）
        UpdateFacilityProduction();
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
        facility.productionState.lastProductionTime = Time.time;

        if (showDebugLog)
        {
            Debug.Log($"Facility '{facility.facilityName}' registered for production.");
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
    /// すべてのFacilityの生成状態を更新
    /// </summary>
    private void UpdateFacilityProduction()
    {
        foreach (var facility in managedFacilities)
        {
            // 解放されていない、または設定が無効な場合はスキップ
            if (!facility.isUnlocked || !facility.productionConfig.IsValid())
                continue;

            // 一時停止中はスキップ
            if (facility.productionState.isPaused)
                continue;

            // 時間チェック（現在はTime.timeを使用）
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
    /// Facilityのリソースを生成
    /// </summary>
    private void ProduceFacilityResources(Facility facility)
    {
        int moneyProduced = 0;
        Dictionary<string, int> itemsProduced = new Dictionary<string, int>();

        // お金の生成
        if (facility.productionConfig.ProducesMoney())
        {
            moneyProduced = facility.productionConfig.moneyAmount;
            
            // TODO: GameDatabaseにお金を追加
            // GameDatabase.Instance.AddMoney(moneyProduced);
            
            if (showDebugLog)
            {
                Debug.Log($"[{facility.facilityName}] お金を生成: {moneyProduced}");
            }
        }

        // アイテムの生成
        if (facility.productionConfig.ProducesItems())
        {
            foreach (var itemProd in facility.productionConfig.itemProductions)
            {
                string itemId = itemProd.GetItemId();
                int quantity = itemProd.quantity;

                // TODO: GameDatabaseにアイテムを追加
                // GameDatabase.Instance.AddItem(itemId, quantity);

                itemsProduced[itemId] = quantity;

                if (showDebugLog)
                {
                    Debug.Log($"[{facility.facilityName}] アイテムを生成: {itemId} x{quantity}");
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
}
