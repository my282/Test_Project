using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 問題データ専用のマスターデータベース
/// 既存のMasterDatabaseとは独立して動作
/// </summary>
[CreateAssetMenu(fileName = "ChallengeMasterDatabase", menuName = "Game/Challenge Master Database")]
public class ChallengeMasterDatabase : ScriptableObject
{
    private static ChallengeMasterDatabase _instance;

    /// <summary>
    /// シングルトンインスタンス（Resourcesフォルダから読み込み）
    /// </summary>
    public static ChallengeMasterDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ChallengeMasterDatabase>("ChallengeMasterDatabase");
                if (_instance == null)
                {
                    Debug.LogError("ChallengeMasterDatabaseが見つかりません！Resourcesフォルダに配置してください。");
                }
            }
            return _instance;
        }
    }

    [Header("問題マスターデータ")]
    [SerializeField] private List<ChallengeData> allChallenges = new List<ChallengeData>();

    #region 問題マスター

    /// <summary>
    /// すべての問題データを取得
    /// </summary>
    public List<ChallengeData> GetAllChallengeData()
    {
        return allChallenges.Where(c => c != null).ToList();
    }

    /// <summary>
    /// IDで問題データを取得
    /// </summary>
    public ChallengeData GetChallengeData(string challengeId)
    {
        return allChallenges.Find(challenge => challenge != null && challenge.challengeId == challengeId);
    }

    /// <summary>
    /// タイプ別に問題データを取得
    /// </summary>
    public List<ChallengeData> GetChallengeDataByType(ChallengeType type)
    {
        return allChallenges.Where(c => c != null && c.type == type).ToList();
    }

    /// <summary>
    /// 難易度別に問題データを取得
    /// </summary>
    public List<ChallengeData> GetChallengeDataByDifficulty(ChallengeDifficulty difficulty)
    {
        return allChallenges.Where(c => c != null && c.difficulty == difficulty).ToList();
    }

    /// <summary>
    /// 難易度別にランダムで問題データを取得
    /// </summary>
    public ChallengeData GetRandomChallengeByDifficulty(ChallengeDifficulty difficulty)
    {
        return ChallengeDatabaseHelper.GetRandomChallengeByDifficulty(allChallenges, difficulty);
    }

    /// <summary>
    /// 問題データを追加（エディタまたは実行時）
    /// </summary>
    public void AddChallengeData(ChallengeData challengeData)
    {
        if (challengeData == null)
        {
            Debug.LogWarning("null の問題データは追加できません。");
            return;
        }
        
        if (!allChallenges.Contains(challengeData))
        {
            allChallenges.Add(challengeData);
            Debug.Log($"問題データ「{challengeData.challengeName}」を追加しました。");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else
        {
            Debug.LogWarning($"問題データ「{challengeData.challengeName}」は既に存在します。");
        }
    }

    /// <summary>
    /// 問題データを削除（エディタ専用）
    /// </summary>
    public void RemoveChallengeData(ChallengeData challengeData)
    {
        if (allChallenges.Contains(challengeData))
        {
            allChallenges.Remove(challengeData);
            Debug.Log($"問題データ「{challengeData.challengeName}」を削除しました。");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else
        {
            Debug.LogWarning($"問題データ「{challengeData.challengeName}」は存在しません。");
        }
    }

    /// <summary>
    /// 問題データをクリア（エディタ専用）
    /// </summary>
    public void ClearAllChallenges()
    {
        allChallenges.Clear();
        Debug.Log("すべての問題データをクリアしました。");
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /// <summary>
    /// 問題データの総数を取得
    /// </summary>
    public int GetChallengeCount()
    {
        return allChallenges.Count(c => c != null);
    }

    /// <summary>
    /// リストからnull要素を削除（エディタ専用）
    /// </summary>
    public int CleanupNullEntries()
    {
        int beforeCount = allChallenges.Count;
        allChallenges.RemoveAll(c => c == null);
        int removedCount = beforeCount - allChallenges.Count;
        
        if (removedCount > 0)
        {
            Debug.LogWarning($"ChallengeMasterDatabase: {removedCount}個のnull要素を削除しました。");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
        return removedCount;
    }

    /// <summary>
    /// 難易度別の問題数を取得
    /// </summary>
    public int GetChallengeCountByDifficulty(ChallengeDifficulty difficulty)
    {
        return allChallenges.Count(c => c != null && c.difficulty == difficulty);
    }

    /// <summary>
    /// タイプ別の問題数を取得
    /// </summary>
    public int GetChallengeCountByType(ChallengeType type)
    {
        return allChallenges.Count(c => c != null && c.type == type);
    }

    /// <summary>
    /// 統計情報を取得
    /// </summary>
    public string GetStatistics()
    {
        int totalCount = GetChallengeCount();
        
        if (totalCount == 0)
        {
            return "問題データがありません。";
        }

        string stats = $"=== Challenge Master Database 統計 ===\n";
        stats += $"総問題数: {totalCount}\n\n";

        stats += "【難易度別】\n";
        foreach (ChallengeDifficulty diff in System.Enum.GetValues(typeof(ChallengeDifficulty)))
        {
            int count = GetChallengeCountByDifficulty(diff);
            stats += $"  {diff}: {count}問\n";
        }

        stats += "\n【種類別】\n";
        foreach (ChallengeType type in System.Enum.GetValues(typeof(ChallengeType)))
        {
            int count = GetChallengeCountByType(type);
            stats += $"  {type}: {count}問\n";
        }

        return stats;
    }

    #endregion
}
