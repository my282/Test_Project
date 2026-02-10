using UnityEngine;
using TMPro;

/// <summary>
/// 総資産を表示する例（将来の拡張用サンプル）
/// PersistentInfoDisplayを継承して使用
/// </summary>
public class TotalAssetsDisplay : PersistentInfoDisplay
{
    [Header("資産表示設定")]
    [SerializeField] private string currencySymbol = "¥";
    [SerializeField] private bool useThousandSeparator = true;

    // 現在の総資産（実際にはGameDatabaseなどから取得）
    private long currentAssets = 0;

    protected override void InitializeDisplay()
    {
        displayPrefix = "総資産: ";
        base.InitializeDisplay();
    }

    private void Update()
    {
        // 実際の実装では、GameDatabaseなどから資産情報を取得
        // 例: currentAssets = GameDatabase.Instance.GetTotalAssets();
        
        UpdateDisplay();
    }

    protected override void UpdateDisplay()
    {
        string formattedAssets = FormatCurrency(currentAssets);
        SetText(formattedAssets);
    }

    protected override string GetDisplayKey()
    {
        return "TotalAssets";
    }

    /// <summary>
    /// 通貨フォーマット
    /// </summary>
    private string FormatCurrency(long amount)
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

    /// <summary>
    /// 資産を設定（テスト用）
    /// </summary>
    public void SetAssets(long amount)
    {
        currentAssets = amount;
    }

    /// <summary>
    /// 資産を追加（テスト用）
    /// </summary>
    public void AddAssets(long amount)
    {
        currentAssets += amount;
    }
}
