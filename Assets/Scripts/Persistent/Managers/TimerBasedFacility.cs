using UnityEngine;

/// <summary>
/// Facilityからタイマーを利用する例
/// 一定時間ごとにアイテムを生成する仕組みの基盤
/// </summary>
public class TimerBasedFacility : MonoBehaviour
{
    [Header("生成設定")]
    [Tooltip("アイテム生成間隔（秒）")]
    [SerializeField] private float productionInterval = 10f;

    [Tooltip("自動生成を有効化")]
    [SerializeField] private bool autoProduction = true;

    // 最後に生成した時間を記録
    private float lastProductionTime;

    // 生成カウンター
    private int productionCount = 0;

    private void Start()
    {
        // TimerManagerが存在するか確認
        if (TimerManager.Instance == null)
        {
            Debug.LogWarning("TimerManager が見つかりません。");
            return;
        }

        // 初期化
        lastProductionTime = TimerManager.Instance.CurrentTime;

        // タイマー開始イベントに登録
        TimerManager.Instance.OnTimerStarted += OnTimerStarted;
        TimerManager.Instance.OnTimerStopped += OnTimerStopped;
    }

    private void OnDestroy()
    {
        // イベント解除
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTimerStarted -= OnTimerStarted;
            TimerManager.Instance.OnTimerStopped -= OnTimerStopped;
        }
    }

    private void Update()
    {
        if (!autoProduction || TimerManager.Instance == null)
            return;

        // タイマーが動作中でない場合はスキップ
        if (!TimerManager.Instance.IsRunning || TimerManager.Instance.IsPaused)
            return;

        CheckProduction();
    }

    /// <summary>
    /// 生成チェック
    /// </summary>
    private void CheckProduction()
    {
        // TimerManagerのCheckIntervalメソッドを使用
        if (TimerManager.Instance.CheckInterval(productionInterval, ref lastProductionTime))
        {
            ProduceItem();
        }
    }

    /// <summary>
    /// アイテムを生成（サブクラスでオーバーライド）
    /// </summary>
    protected virtual void ProduceItem()
    {
        productionCount++;
        Debug.Log($"アイテムを生成しました！（生成数: {productionCount}）");
        
        // ここで実際のアイテム生成処理を実装
        // 例: GameDatabase.Instance.AddItem(...);
    }

    /// <summary>
    /// タイマー開始時の処理
    /// </summary>
    private void OnTimerStarted()
    {
        Debug.Log($"{gameObject.name}: タイマー開始を検知");
        lastProductionTime = TimerManager.Instance.CurrentTime;
    }

    /// <summary>
    /// タイマー停止時の処理
    /// </summary>
    private void OnTimerStopped()
    {
        Debug.Log($"{gameObject.name}: タイマー停止を検知");
    }

    /// <summary>
    /// 生成間隔を変更
    /// </summary>
    public void SetProductionInterval(float interval)
    {
        productionInterval = Mathf.Max(0.1f, interval);
    }

    /// <summary>
    /// 自動生成のON/OFF
    /// </summary>
    public void SetAutoProduction(bool enabled)
    {
        autoProduction = enabled;
    }

    /// <summary>
    /// 手動でアイテムを生成
    /// </summary>
    public void ManualProduce()
    {
        ProduceItem();
    }

    /// <summary>
    /// 生成カウントをリセット
    /// </summary>
    public void ResetProductionCount()
    {
        productionCount = 0;
    }

    /// <summary>
    /// 現在の生成カウントを取得
    /// </summary>
    public int GetProductionCount()
    {
        return productionCount;
    }
}
