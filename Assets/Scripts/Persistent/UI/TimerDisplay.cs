using UnityEngine;
using TMPro;

/// <summary>
/// タイマー表示用UIコンポーネント
/// </summary>
public class TimerDisplay : MonoBehaviour
{
    [Header("UI参照")]
    [Tooltip("タイマーを表示するTextコンポーネント")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("表示設定")]
    [Tooltip("詳細表示（HH:MM:SS形式）を使用")]
    [SerializeField] private bool useDetailedFormat = false;

    [Tooltip("時間切れ時の警告色")]
    [SerializeField] private Color warningColor = Color.red;

    [Tooltip("通常時の色")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("警告を表示する残り時間（秒）")]
    [SerializeField] private float warningThreshold = 30f;

    [Tooltip("時間切れ時に点滅")]
    [SerializeField] private bool blinkOnWarning = true;

    [Tooltip("点滅間隔（秒）")]
    [SerializeField] private float blinkInterval = 0.5f;

    private float blinkTimer;
    private bool isBlinking;

    private void Start()
    {
        // TimerManagerのイベントに登録
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTimerUpdated += UpdateDisplay;
            TimerManager.Instance.OnTimerFinished += OnTimerFinished;
        }

        // 初期表示
        UpdateDisplay(0);
    }

    private void OnDestroy()
    {
        // イベント解除
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTimerUpdated -= UpdateDisplay;
            TimerManager.Instance.OnTimerFinished -= OnTimerFinished;
        }
    }

    private void Update()
    {
        if (isBlinking && blinkOnWarning)
        {
            HandleBlinking();
        }
    }

    /// <summary>
    /// タイマー表示を更新
    /// </summary>
    private void UpdateDisplay(float currentTime)
    {
        if (timerText == null)
            return;

        // テキスト更新
        if (useDetailedFormat)
        {
            timerText.text = TimerManager.Instance.GetFormattedTimeDetailed();
        }
        else
        {
            timerText.text = TimerManager.Instance.GetFormattedTime();
        }

        // 色の更新（カウントダウンモードのみ）
        if (TimerManager.Instance.Mode == TimerManager.TimerMode.CountDown)
        {
            float remainingTime = TimerManager.Instance.RemainingTime;
            
            if (remainingTime <= warningThreshold)
            {
                isBlinking = true;
                if (!blinkOnWarning)
                {
                    timerText.color = warningColor;
                }
            }
            else
            {
                isBlinking = false;
                timerText.color = normalColor;
            }
        }
    }

    /// <summary>
    /// 点滅処理
    /// </summary>
    private void HandleBlinking()
    {
        blinkTimer += Time.deltaTime;
        
        if (blinkTimer >= blinkInterval)
        {
            blinkTimer = 0;
            timerText.color = timerText.color == warningColor ? normalColor : warningColor;
        }
    }

    /// <summary>
    /// タイマー終了時の処理
    /// </summary>
    private void OnTimerFinished()
    {
        if (timerText != null)
        {
            timerText.color = warningColor;
        }
    }

    /// <summary>
    /// 表示フォーマットを切り替え
    /// </summary>
    public void ToggleFormat()
    {
        useDetailedFormat = !useDetailedFormat;
        UpdateDisplay(TimerManager.Instance.CurrentTime);
    }
}
