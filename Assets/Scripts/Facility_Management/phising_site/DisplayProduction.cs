using UnityEngine;
using TMPro;

/// <summary>
/// Script to display facility auto-production information
/// - Shows production interval and quantity
/// - Select target facility by ID
/// - Toggle interval display on/off
/// </summary>
public class DisplayProduction : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Facility ID to display")]
    [SerializeField] private string facilityId = "phishing_site";
    
    [Tooltip("Show production interval")]
    [SerializeField] private bool showInterval = true;
    
    [Tooltip("Custom name for item display (default: 'Item')")]
    [SerializeField] private string itemDisplayName = "Item";
    
    [Header("UI Reference")]
    [Tooltip("Text component for display (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI textComponent;
    
    [Tooltip("Text component for display (Legacy Text)")]
    [SerializeField] private UnityEngine.UI.Text legacyTextComponent;
    
    // Cache
    private int cachedLevel = -1;
    private Facility cachedFacility;
    
    void Start()
    {
        // Initial display
        UpdateDisplay();
    }
    
    void Update()
    {
        // Lightweight check: Update display if level changed
        if (cachedFacility != null && cachedFacility.level != cachedLevel)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// Update display (can be called externally)
    /// </summary>
    public void UpdateDisplay()
    {
        // Get facility information from GameDatabase
        if (GameDatabase.Instance == null)
        {
            SetDisplayText("GameDatabase not found");
            return;
        }
        
        Facility facility = GameDatabase.Instance.GetFacility(facilityId);
        
        if (facility == null)
        {
            SetDisplayText($"Facility '{facilityId}' not found");
            return;
        }
        
        // If facility is not unlocked
        if (!facility.isUnlocked)
        {
            SetDisplayText($"{facility.facilityName ?? facilityId}\nLocked");
            return;
        }
        
        // If auto production is disabled
        if (!facility.productionConfig.enableAutoProduction)
        {
            SetDisplayText($"{facility.facilityName ?? facilityId}\nProduction: Disabled");
            return;
        }
        
        // Cache update
        cachedFacility = facility;
        cachedLevel = facility.level;
        
        // Build display text
        string displayText = BuildDisplayText(facility);
        SetDisplayText(displayText);
    }
    
    /// <summary>
    /// Build display text
    /// </summary>
    private string BuildDisplayText(Facility facility)
    {
        string text = "";
        
        // Facility name and level
        text += $"{facility.facilityName ?? facilityId} Lv.{facility.level}\n";
        
        // Production interval (if display setting is on)
        if (showInterval)
        {
            text += $"Interval: {facility.productionConfig.productionInterval}s\n";
        }
        
        // Production content
        text += "Production: ";
        
        bool hasContent = false;
        
        // Money production
        if (facility.productionConfig.ProducesMoney())
        {
            int moneyAmount = Mathf.RoundToInt(facility.productionConfig.moneyAmount * facility.GetProductionMultiplier());
            text += $"${moneyAmount}";
            hasContent = true;
        }
        
        // Item production
        if (facility.productionConfig.ProducesItems() && facility.productionConfig.itemProductions != null)
        {
            foreach (var item in facility.productionConfig.itemProductions)
            {
                if (hasContent)
                {
                    text += ", ";
                }
                
                int quantity = Mathf.RoundToInt(item.quantity * facility.GetProductionMultiplier());
                text += $"{itemDisplayName} x{quantity}";
                hasContent = true;
            }
        }
        
        if (!hasContent)
        {
            text += "None";
        }
        
        return text;
    }
    
    /// <summary>
    /// Set text to component
    /// </summary>
    private void SetDisplayText(string text)
    {
        // If TextMeshPro is set
        if (textComponent != null)
        {
            textComponent.text = text;
        }
        // If legacy Text is set
        else if (legacyTextComponent != null)
        {
            legacyTextComponent.text = text;
        }
        // If neither is set, output to log
        else
        {
            Debug.Log($"[DisplayProduction] {text}");
        }
    }
    
    /// <summary>
    /// Change Facility ID (can be set externally)
    /// </summary>
    public void SetFacilityId(string newFacilityId)
    {
        facilityId = newFacilityId;
        cachedLevel = -1; // Reset cache
        UpdateDisplay();
    }
    
    /// <summary>
    /// Toggle interval display on/off (can be set externally)
    /// </summary>
    public void SetShowInterval(bool show)
    {
        showInterval = show;
        UpdateDisplay();
    }
}
