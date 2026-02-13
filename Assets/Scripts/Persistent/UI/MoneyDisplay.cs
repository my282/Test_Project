using UnityEngine;
using TMPro;

/// <summary>
/// 所持金を表示するUIコンポーネント
/// GameDatabaseのイベントに連携して自動更新
/// </summary>
public class MoneyDisplay : PersistentInfoDisplay
{
    [Header("所持金表示設定")]
    [SerializeField] private string currencySymbol = "¥";
    [SerializeField] private bool useThousandSeparator = true;

    protected override void Start()
    {
        base.Start();
        
        // GameDatabaseのイベントに登録
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnMoneyChanged += UpdateDisplay;
        }
        
        // 初期表示を更新
        UpdateDisplay();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // イベント解除
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.OnMoneyChanged -= UpdateDisplay;
        }
    }

    protected override void InitializeDisplay()
    {
        displayPrefix = "money: ";
        base.InitializeDisplay();
    }

    protected override void UpdateDisplay()
    {
        if (GameDatabase.Instance == null)
        {
            SetText("---");
            return;
        }
        
        int money = GameDatabase.Instance.GetMoney();
        string formattedMoney = FormatCurrency(money);
        SetText(formattedMoney);
    }

    protected override string GetDisplayKey()
    {
        return "Money";
    }

    /// <summary>
    /// 通貨フォーマット
    /// </summary>
    private string FormatCurrency(int amount)
    {
        string formatted;
        
        if (useThousandSeparator)
        {
            formatted = amount.ToString("N0"); // カンマ区切り
        }
        else
        {
            formatted = amount.ToString();
        }

        return currencySymbol + formatted;
    }
}
