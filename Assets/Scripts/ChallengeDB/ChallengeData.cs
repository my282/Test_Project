using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 問題のマスターデータ（ScriptableObject）
/// ItemDataと同様のパターンで実装
/// </summary>
[CreateAssetMenu(fileName = "NewChallenge", menuName = "Game/Challenge Data")]
public class ChallengeData : ScriptableObject
{
    [Header("基本情報")]
    [Tooltip("問題の一意なID（例: challenge_math_001）")]
    public string challengeId;
    
    [Tooltip("問題の表示名")]
    public string challengeName;
    
    [TextArea(3, 5)]
    [Tooltip("問題の説明")]
    public string description;
    
    [Header("難易度・種類")]
    [Tooltip("問題の難易度（5段階）")]
    public ChallengeDifficulty difficulty = ChallengeDifficulty.Normal;
    
    [Tooltip("問題の種類")]
    public ChallengeType type = ChallengeType.Math;
    
    [Header("問題内容")]
    [TextArea(5, 10)]
    [Tooltip("問題文")]
    public string questionText;
    
    [Header("回答設定")]
    [Tooltip("回答の入力タイプ")]
    public AnswerType answerType = AnswerType.Text;
    
    [Tooltip("正解パターン（複数設定可能）")]
    public string[] correctAnswers = new string[1];
    
    [Header("ヒント")]
    [TextArea(2, 4)]
    [Tooltip("ヒント（オプション）")]
    public string hint;
    
    [Header("報酬設定")]
    [Tooltip("クリア時の報酬テーブル")]
    public RewardTable rewardTable = new RewardTable();
    
    /// <summary>
    /// Challengeインスタンスを生成（ファクトリーメソッド）
    /// ItemData.CreateItem()と同様のパターン
    /// </summary>
    public Challenge CreateChallenge()
    {
        return new Challenge(
            challengeId,
            challengeName,
            description,
            difficulty,
            type,
            questionText,
            answerType,
            correctAnswers,
            hint
        );
    }
    
    /// <summary>
    /// データの妥当性をチェック
    /// </summary>
    public bool Validate(out string errorMessage)
    {
        if (string.IsNullOrEmpty(challengeId))
        {
            errorMessage = "Challenge IDが設定されていません。";
            return false;
        }
        
        if (string.IsNullOrEmpty(challengeName))
        {
            errorMessage = "Challenge名が設定されていません。";
            return false;
        }
        
        if (string.IsNullOrEmpty(questionText))
        {
            errorMessage = "問題文が設定されていません。";
            return false;
        }
        
        if (correctAnswers == null || correctAnswers.Length == 0)
        {
            errorMessage = "正解パターンが設定されていません。";
            return false;
        }
        
        // 空の正解パターンがないかチェック
        foreach (var answer in correctAnswers)
        {
            if (string.IsNullOrEmpty(answer))
            {
                errorMessage = "空の正解パターンが含まれています。";
                return false;
            }
        }
        
        errorMessage = null;
        return true;
    }
}
