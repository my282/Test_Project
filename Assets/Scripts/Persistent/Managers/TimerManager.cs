using UnityEngine;
using System;

/// <summary>
/// ゲーム全体のタイマーを管理するシングルトンクラス
/// シーンを跨いで永続化され、どこからでもアクセス可能
/// </summary>
public class TimerManager : MonoBehaviour
{
    private static TimerManager instance;
    public static TimerManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TimerManager");
                instance = go.AddComponent<TimerManager>();
            }
            return instance;
        }
    }

    [Header("タイマー設定")]
    [Tooltip("ゲーム開始時の制限時間（秒）")]
    [SerializeField] private float initialTime = 300f; // デフォルト5分

    [Tooltip("タイマーの動作モード")]
    [SerializeField] private TimerMode timerMode = TimerMode.CountDown;

    [Tooltip("タイマーを自動開始")]
    [SerializeField] private bool autoStart = true;

    // 現在の経過時間
    private float currentTime;
    
    // タイマーが動作中か
    private bool isRunning;
    
    // タイマーが一時停止中か
    private bool isPaused;

    // タイマー更新イベント（UI更新用）
    public event Action<float> OnTimerUpdated;
    
    // タイマー終了イベント
    public event Action OnTimerFinished;
    
    // タイマー開始イベント
    public event Action OnTimerStarted;
    
    // タイマー停止イベント
    public event Action OnTimerStopped;
    
    // タイマー一時停止イベント
    public event Action<bool> OnTimerPausedChanged;

    public enum TimerMode
    {
        CountDown,  // カウントダウン
        CountUp     // カウントアップ
    }

    // プロパティ
    public float CurrentTime => currentTime;
    public float InitialTime => initialTime;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public TimerMode Mode => timerMode;

    /// <summary>
    /// 残り時間（カウントダウンの場合）
    /// </summary>
    public float RemainingTime
    {
        get
        {
            if (timerMode == TimerMode.CountDown)
                return Mathf.Max(0, initialTime - currentTime);
            return 0;
        }
    }

    /// <summary>
    /// 残り時間の割合（0.0 - 1.0）
    /// </summary>
    public float TimeRatio
    {
        get
        {
            if (timerMode == TimerMode.CountDown && initialTime > 0)
                return RemainingTime / initialTime;
            return 0;
        }
    }

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

        InitializeTimer();
    }

    private void Start()
    {
        if (autoStart)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isRunning || isPaused)
            return;

        UpdateTimer();
    }

    /// <summary>
    /// タイマーの初期化
    /// </summary>
    private void InitializeTimer()
    {
        currentTime = 0f;
        isRunning = false;
        isPaused = false;
    }

    /// <summary>
    /// タイマーの更新処理
    /// </summary>
    private void UpdateTimer()
    {
        currentTime += Time.deltaTime;

        // イベント通知
        OnTimerUpdated?.Invoke(currentTime);

        // カウントダウンモードで時間切れチェック
        if (timerMode == TimerMode.CountDown && currentTime >= initialTime)
        {
            FinishTimer();
        }
    }

    /// <summary>
    /// タイマーを開始
    /// </summary>
    public void StartTimer()
    {
        if (isRunning)
            return;

        isRunning = true;
        isPaused = false;
        OnTimerStarted?.Invoke();
        Debug.Log("タイマーを開始しました");
    }

    /// <summary>
    /// タイマーを停止
    /// </summary>
    public void StopTimer()
    {
        if (!isRunning)
            return;

        isRunning = false;
        isPaused = false;
        OnTimerStopped?.Invoke();
        Debug.Log("タイマーを停止しました");
    }

    /// <summary>
    /// タイマーを一時停止/再開
    /// </summary>
    public void TogglePause()
    {
        if (!isRunning)
            return;

        isPaused = !isPaused;
        OnTimerPausedChanged?.Invoke(isPaused);
        Debug.Log(isPaused ? "タイマーを一時停止しました" : "タイマーを再開しました");
    }

    /// <summary>
    /// タイマーをリセット
    /// </summary>
    public void ResetTimer()
    {
        currentTime = 0f;
        isPaused = false;
        OnTimerUpdated?.Invoke(currentTime);
        Debug.Log("タイマーをリセットしました");
    }

    /// <summary>
    /// タイマーを終了
    /// </summary>
    private void FinishTimer()
    {
        isRunning = false;
        currentTime = initialTime;
        OnTimerFinished?.Invoke();
        Debug.Log("タイマーが終了しました");
    }

    /// <summary>
    /// 制限時間を設定
    /// </summary>
    public void SetInitialTime(float time)
    {
        initialTime = Mathf.Max(0, time);
        if (currentTime > initialTime)
        {
            currentTime = initialTime;
        }
    }

    /// <summary>
    /// タイマーモードを変更
    /// </summary>
    public void SetTimerMode(TimerMode mode)
    {
        timerMode = mode;
    }

    /// <summary>
    /// 時間を追加（ボーナスタイムなど）
    /// </summary>
    public void AddTime(float additionalTime)
    {
        if (timerMode == TimerMode.CountDown)
        {
            initialTime += additionalTime;
            Debug.Log($"{additionalTime}秒追加されました");
        }
    }

    /// <summary>
    /// 時間表示用の文字列を取得（MM:SS形式）
    /// </summary>
    public string GetFormattedTime()
    {
        float displayTime = timerMode == TimerMode.CountDown ? RemainingTime : currentTime;
        int minutes = Mathf.FloorToInt(displayTime / 60f);
        int seconds = Mathf.FloorToInt(displayTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// 詳細な時間表示用の文字列を取得（HH:MM:SS形式）
    /// </summary>
    public string GetFormattedTimeDetailed()
    {
        float displayTime = timerMode == TimerMode.CountDown ? RemainingTime : currentTime;
        int hours = Mathf.FloorToInt(displayTime / 3600f);
        int minutes = Mathf.FloorToInt((displayTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(displayTime % 60f);
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// 特定の時間間隔でコールバックを実行（Facilityなどで利用）
    /// </summary>
    public bool CheckInterval(float interval, ref float lastCheckTime)
    {
        if (currentTime - lastCheckTime >= interval)
        {
            lastCheckTime = currentTime;
            return true;
        }
        return false;
    }
}
