using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class build_phishing_site : MonoBehaviour
{
    [Header("Display Reference")]
    [Tooltip("DisplayProduction component to update after building")]
    [SerializeField] private DisplayProduction displayProduction;
    
    void Start()
    {

    }

    void Update()
    {

    }

    public void build()
    {
        // 設備を解放
        GameDatabase.Instance.UnlockFacilityWithCost("phishing_site");
        
        // 自動生成を登録（FacilityDataの設定を使用）
        RegisterAutoProduction();
        
        // TimerManagerを開始（重要！）
        StartTimerIfNeeded();
        
        // FacilityProductionControllerの状態を確認
        CheckProductionController();
        
        // DisplayProductionを更新
        UpdateDisplayProduction();
    }
    
    /// <summary>
    /// Update DisplayProduction after building facility
    /// </summary>
    void UpdateDisplayProduction()
    {
        if (displayProduction != null)
        {
            displayProduction.UpdateDisplay();
            Debug.Log("✅ DisplayProduction updated");
        }
        else
        {
            // If not assigned in Inspector, try to find it in the scene
            displayProduction = FindObjectOfType<DisplayProduction>();
            if (displayProduction != null)
            {
                displayProduction.UpdateDisplay();
                Debug.Log("✅ DisplayProduction found and updated");
            }
            else
            {
                Debug.LogWarning("⚠️ DisplayProduction not found");
            }
        }
    }
    
    /// <summary>
    /// TimerManagerを開始（自動生成に必要）
    /// </summary>
    void StartTimerIfNeeded()
    {
        if (TimerManager.Instance != null)
        {
            if (!TimerManager.Instance.IsRunning)
            {
                TimerManager.Instance.StartTimer();
                Debug.Log("✅ TimerManagerを開始しました");
            }
            else
            {
                Debug.Log("✅ TimerManagerは既に実行中です");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ TimerManagerが見つかりません。Time.timeでフォールバック動作します。");
        }
    }
    
    /// <summary>
    /// FacilityProductionControllerの状態を確認
    /// </summary>
    void CheckProductionController()
    {
        if (FacilityProductionController.Instance == null)
        {
            Debug.LogError("❌ FacilityProductionControllerが見つかりません！");
            Debug.LogError("   → 自動作成を試みます...");
            CreateProductionController();
            return;
        }
        
        Debug.Log($"✅ FacilityProductionController: 存在確認OK");
    }
    
    /// <summary>
    /// FacilityProductionControllerを自動作成
    /// </summary>
    void CreateProductionController()
    {
        GameObject go = new GameObject("FacilityProductionController");
        var controller = go.AddComponent<FacilityProductionController>();
        
        // showDebugLogをtrueに設定（リフレクション使用）
        var field = controller.GetType().GetField("showDebugLog", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(controller, true);
        }
        
        Debug.Log("✅ FacilityProductionControllerを自動作成しました（DontDestroyOnLoad設定済み）");
        
        // 再度登録を試みる
        RegisterAutoProduction();
    }
    
    /// <summary>
    /// phishing_siteの自動生成をコントローラーに登録
    /// ※ 生成設定はFacilityData（Inspector）で設定済み
    /// </summary>
    void RegisterAutoProduction()
    {
        Facility facilityA = GameDatabase.Instance.GetFacility("phishing_site");
        
        if (facilityA == null)
        {
            Debug.LogError("phishing_siteが見つかりません！");
            return;
        }
        
        if (!facilityA.isUnlocked)
        {
            Debug.LogError("phishing_siteが解放されていません！");
            return;
        }
        
        // デバッグ: 生成設定を確認
        Debug.Log($"=== phishing_site 生成設定確認 ===");
        Debug.Log($"Enable Auto Production: {facilityA.productionConfig.enableAutoProduction}");
        Debug.Log($"Production Type: {facilityA.productionConfig.productionType}");
        Debug.Log($"Production Interval: {facilityA.productionConfig.productionInterval}秒");
        Debug.Log($"Money Amount: {facilityA.productionConfig.moneyAmount}");
        Debug.Log($"Item Productions Count: {facilityA.productionConfig.itemProductions?.Count ?? 0}");
        
        if (facilityA.productionConfig.itemProductions != null)
        {
            foreach (var item in facilityA.productionConfig.itemProductions)
            {
                Debug.Log($"  - Item: {item.itemId} x{item.quantity}");
            }
        }
        
        // FacilityDataで設定した内容がそのまま使われる
        if (facilityA.productionConfig.enableAutoProduction)
        {
            // コントローラーに登録するだけ
            if (FacilityProductionController.Instance != null)
            {
                FacilityProductionController.Instance.RegisterFacility(facilityA);
                
                // 設定内容をログ表示
                string productionInfo = GetProductionInfo(facilityA);
                Debug.Log($"✅ phishing_siteの自動生成を開始: {productionInfo}");
            }
            else
            {
                Debug.LogWarning("FacilityProductionControllerが見つかりません。");
            }
        }
        else
        {
            Debug.Log("❌ phishing_siteの自動生成は無効になっています（FacilityDataで Enable Auto Production をONにしてください）");
        }
    }
    
    /// <summary>
    /// 生成設定の情報を文字列で取得
    /// </summary>
    string GetProductionInfo(Facility facility)
    {
        var config = facility.productionConfig;
        string info = $"{config.productionInterval}秒ごとに";
        
        if (config.ProducesMoney())
        {
            info += $"お金{config.moneyAmount}";
        }
        
        if (config.ProducesItems())
        {
            if (config.ProducesMoney()) info += "、";
            info += "アイテム[";
            foreach (var item in config.itemProductions)
            {
                info += $"{item.itemId}×{item.quantity} ";
            }
            info += "]";
        }
        
        return info + "を生成";
    }
    
    /// <summary>
    /// デバッグ: 自動生成の状態を確認（Updateメニューから実行可能）
    /// </summary>
    [ContextMenu("自動生成状態を確認")]
    void DebugProductionStatus()
    {
        Debug.Log("========== 自動生成状態確認 ==========");
        
        // TimerManagerの状態
        if (TimerManager.Instance != null)
        {
            Debug.Log($"[TimerManager]");
            Debug.Log($"  IsRunning: {TimerManager.Instance.IsRunning}");
            Debug.Log($"  IsPaused: {TimerManager.Instance.IsPaused}");
            Debug.Log($"  CurrentTime: {TimerManager.Instance.CurrentTime}");
        }
        else
        {
            Debug.LogWarning("[TimerManager] 存在しません（Time.timeでフォールバック）");
        }
        
        // FacilityProductionControllerの状態
        if (FacilityProductionController.Instance != null)
        {
            Debug.Log($"[FacilityProductionController] 存在確認OK");
        }
        else
        {
            Debug.LogError("[FacilityProductionController] 存在しません！");
            return;
        }
        
        // phishing_siteの状態
        Facility facilityA = GameDatabase.Instance.GetFacility("phishing_site");
        if (facilityA != null)
        {
            Debug.Log($"[phishing_site]");
            Debug.Log($"  isUnlocked: {facilityA.isUnlocked}");
            Debug.Log($"  isPaused: {facilityA.productionState.isPaused}");
            Debug.Log($"  enableAutoProduction: {facilityA.productionConfig.enableAutoProduction}");
            Debug.Log($"  IsValid: {facilityA.productionConfig.IsValid()}");
            Debug.Log($"  productionType: {facilityA.productionConfig.productionType}");
            Debug.Log($"  productionInterval: {facilityA.productionConfig.productionInterval}秒");
            Debug.Log($"  lastProductionTime: {facilityA.productionState.lastProductionTime}");
            Debug.Log($"  totalProductionCount: {facilityA.productionState.totalProductionCount}");
            
            float currentTime = TimerManager.Instance != null ? 
                TimerManager.Instance.CurrentTime : Time.time;
            float elapsed = currentTime - facilityA.productionState.lastProductionTime;
            Debug.Log($"  経過時間: {elapsed:F2}秒 / {facilityA.productionConfig.productionInterval}秒");
            
            if (facilityA.productionConfig.itemProductions != null)
            {
                Debug.Log($"  アイテム数: {facilityA.productionConfig.itemProductions.Count}");
                foreach (var item in facilityA.productionConfig.itemProductions)
                {
                    Debug.Log($"    - {item.itemId} x{item.quantity}");
                }
            }
        }
        else
        {
            Debug.LogError("[phishing_site] 見つかりません");
        }
        
        Debug.Log("====================================");
    }
}