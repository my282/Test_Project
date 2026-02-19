using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// プレイヤーの問題回答履歴を管理するシングルトンクラス
/// ランタイムのみ管理（シーン再起動でリセット）
/// </summary>
public class ChallengeProgressManager : MonoBehaviour
{
    private static ChallengeProgressManager _instance;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static ChallengeProgressManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ChallengeProgressManager");
                _instance = go.AddComponent<ChallengeProgressManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 回答済み問題IDのセット
    /// </summary>
    private HashSet<string> solvedChallengeIds = new HashSet<string>();
    
    /// <summary>
    /// 難易度別の回答済み数カウンター
    /// </summary>
    private Dictionary<ChallengeDifficulty, int> solvedCountByDifficulty = new Dictionary<ChallengeDifficulty, int>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 難易度カウンター初期化
        foreach (ChallengeDifficulty difficulty in System.Enum.GetValues(typeof(ChallengeDifficulty)))
        {
            solvedCountByDifficulty[difficulty] = 0;
        }
    }
    
    /// <summary>
    /// 問題を解決済みとしてマーク
    /// </summary>
    /// <param name="challengeId">問題ID</param>
    /// <param name="difficulty">問題の難易度</param>
    public void MarkAsSolved(string challengeId, ChallengeDifficulty difficulty)
    {
        if (string.IsNullOrEmpty(challengeId))
        {
            Debug.LogWarning("ChallengeProgressManager: challengeIdがnullまたは空です");
            return;
        }
        
        if (solvedChallengeIds.Add(challengeId))
        {
            // 新規追加された場合のみカウントアップ
            if (solvedCountByDifficulty.ContainsKey(difficulty))
            {
                solvedCountByDifficulty[difficulty]++;
            }
            
            Debug.Log($"問題を解決済みにマーク: {challengeId} (難易度: {difficulty})");
        }
    }
    
    /// <summary>
    /// 指定した問題が解決済みかどうかを確認
    /// </summary>
    /// <param name="challengeId">問題ID</param>
    /// <returns>解決済みの場合 true</returns>
    public bool IsSolved(string challengeId)
    {
        return solvedChallengeIds.Contains(challengeId);
    }
    
    /// <summary>
    /// 難易度別の解決済み問題数を取得
    /// </summary>
    /// <param name="difficulty">難易度</param>
    /// <returns>解決済み問題数</returns>
    public int GetSolvedCount(ChallengeDifficulty difficulty)
    {
        return solvedCountByDifficulty.ContainsKey(difficulty) ? solvedCountByDifficulty[difficulty] : 0;
    }
    
    /// <summary>
    /// 全体の解決済み問題数を取得
    /// </summary>
    /// <returns>解決済み問題数</returns>
    public int GetTotalSolvedCount()
    {
        return solvedChallengeIds.Count;
    }
    
    /// <summary>
    /// 未解決の問題のみをフィルタリング
    /// </summary>
    /// <param name="allChallenges">全問題データのリスト</param>
    /// <returns>未解決問題データのリスト</returns>
    public List<ChallengeData> GetUnsolvedChallenges(List<ChallengeData> allChallenges)
    {
        if (allChallenges == null)
        {
            Debug.LogWarning("ChallengeProgressManager: allChallengesがnullです");
            return new List<ChallengeData>();
        }
        
        return allChallenges.Where(challenge => !IsSolved(challenge.challengeId)).ToList();
    }
    
    /// <summary>
    /// 難易度を指定して未解決の問題をフィルタリング
    /// </summary>
    /// <param name="difficulty">難易度</param>
    /// <returns>未解決問題データのリスト</returns>
    public List<ChallengeData> GetUnsolvedChallengesByDifficulty(ChallengeDifficulty difficulty)
    {
        var allChallenges = ChallengeMasterDatabase.Instance.GetChallengeDataByDifficulty(difficulty);
        return GetUnsolvedChallenges(allChallenges);
    }
    
    /// <summary>
    /// 回答履歴をリセット
    /// </summary>
    public void Reset()
    {
        solvedChallengeIds.Clear();
        
        foreach (var key in solvedCountByDifficulty.Keys.ToList())
        {
            solvedCountByDifficulty[key] = 0;
        }
        
        Debug.Log("ChallengeProgressManager: 回答履歴をリセットしました");
    }
    
    /// <summary>
    /// 指定難易度の回答履歴のみをリセット
    /// </summary>
    /// <param name="difficulty">難易度</param>
    public void ResetByDifficulty(ChallengeDifficulty difficulty)
    {
        var allChallenges = ChallengeMasterDatabase.Instance.GetChallengeDataByDifficulty(difficulty);
        
        foreach (var challenge in allChallenges)
        {
            if (solvedChallengeIds.Remove(challenge.challengeId))
            {
                if (solvedCountByDifficulty.ContainsKey(difficulty))
                {
                    solvedCountByDifficulty[difficulty]--;
                }
            }
        }
        
        Debug.Log($"ChallengeProgressManager: {difficulty}の回答履歴をリセットしました");
    }
    
    /// <summary>
    /// デバッグ用：現在の状態を出力
    /// </summary>
    public void PrintStatus()
    {
        Debug.Log("=== ChallengeProgressManager Status ===");
        Debug.Log($"総解決済み問題数: {GetTotalSolvedCount()}");
        
        foreach (ChallengeDifficulty difficulty in System.Enum.GetValues(typeof(ChallengeDifficulty)))
        {
            int solvedCount = GetSolvedCount(difficulty);
            int totalCount = ChallengeMasterDatabase.Instance.GetChallengeCountByDifficulty(difficulty);
            Debug.Log($"{difficulty}: {solvedCount}/{totalCount}問");
        }
    }
}
