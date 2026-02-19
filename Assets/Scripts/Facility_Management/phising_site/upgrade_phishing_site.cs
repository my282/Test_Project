using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class upgrade_phishing_site : MonoBehaviour
{
    [Header("サウンド設定")]
    [SerializeField] private AudioClip upgradeSE;  // アップグレード成功時のSE
    
    private AudioSource audioSource;
    
    void Start()
    {
        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {

    }

    public void upgrade()
    {
        // アップグレードを実行
        bool success = GameDatabase.Instance.UpgradeFacilityWithCost("phishing_site");
        
        if (success)
        {
            Debug.Log("✅ phishing_siteのアップグレードに成功しました！");
            
            // 現在のレベルを取得して表示
            Facility facility = GameDatabase.Instance.GetFacility("phishing_site");
            if (facility != null)
            {
                Debug.Log($"現在のレベル: Lv.{facility.level}");
            }
            
            // SEを再生
            PlayUpgradeSE();
        }
        else
        {
            Debug.LogWarning("❌ phishing_siteのアップグレードに失敗しました（コスト不足または最大レベル）");
        }
    }
    
    /// <summary>
    /// アップグレード成功時のSEを再生
    /// </summary>
    private void PlayUpgradeSE()
    {
        if (audioSource != null && upgradeSE != null)
        {
            audioSource.PlayOneShot(upgradeSE);
            Debug.Log("🔊 アップグレードSEを再生しました");
        }
        else if (upgradeSE == null)
        {
            Debug.Log("ℹ️ アップグレードSEが設定されていません（Inspector で設定可能）");
        }
    }
}