using System;
using UnityEngine;

/// <summary>
/// 問題のランタイムデータ構造
/// Item.csと同様のパターンで実装
/// </summary>
[Serializable]
public class Challenge
{
    [Header("基本情報")]
    public string challengeId;
    public string challengeName;
    public string description;
    public ChallengeDifficulty difficulty;
    public ChallengeType type;
    
    [Header("問題内容")]
    public string questionText;
    
    [Header("回答設定")]
    public AnswerType answerType;
    public string[] correctAnswers;
    public string hint;
    
    [Header("プレイヤーの進行状態")]
    public ChallengeStatus status;
    public int attemptCount;        // 挑戦回数
    public int completedCount;      // クリア回数
    public string lastCompletedTime; // 最終クリア日時
    
    /// <summary>
    /// コンストラクタ（ChallengeDataから生成）
    /// </summary>
    public Challenge(
        string id,
        string name,
        string desc,
        ChallengeDifficulty diff,
        ChallengeType challengeType,
        string question,
        AnswerType ansType,
        string[] answers,
        string hintText
    )
    {
        challengeId = id;
        challengeName = name;
        description = desc;
        difficulty = diff;
        type = challengeType;
        questionText = question;
        answerType = ansType;
        correctAnswers = answers;
        hint = hintText;
        
        // 初期状態
        status = ChallengeStatus.Locked;
        attemptCount = 0;
        completedCount = 0;
        lastCompletedTime = "";
    }
    
    /// <summary>
    /// 回答が正しいかチェック
    /// </summary>
    /// <param name="userAnswer">ユーザーの入力</param>
    /// <returns>正解ならtrue</returns>
    public bool CheckAnswer(string userAnswer)
    {
        if (string.IsNullOrEmpty(userAnswer))
        {
            return false;
        }
        
        // 回答タイプに応じて判定
        switch (answerType)
        {
            case AnswerType.Number:
                return CheckNumericAnswer(userAnswer);
            
            case AnswerType.Text:
                return CheckTextAnswer(userAnswer);
            
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 数値回答のチェック（完全一致）
    /// </summary>
    private bool CheckNumericAnswer(string userAnswer)
    {
        // ユーザーの入力を数値に変換
        if (!float.TryParse(userAnswer, out float userValue))
        {
            return false;
        }
        
        // すべての正解パターンと比較（完全一致）
        foreach (var correctAnswer in correctAnswers)
        {
            if (float.TryParse(correctAnswer, out float correctValue))
            {
                if (Mathf.Approximately(userValue, correctValue))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// テキスト回答のチェック（大小文字区別なし）
    /// </summary>
    private bool CheckTextAnswer(string userAnswer)
    {
        // すべての正解パターンと比較（大小文字区別なし）
        foreach (var correctAnswer in correctAnswers)
        {
            if (userAnswer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 挑戦回数を増やす
    /// </summary>
    public void IncrementAttempt()
    {
        attemptCount++;
    }
    
    /// <summary>
    /// クリア記録を更新
    /// </summary>
    public void RecordCompletion()
    {
        status = ChallengeStatus.Completed;
        completedCount++;
        lastCompletedTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }
    
    /// <summary>
    /// 問題を解放
    /// </summary>
    public void Unlock()
    {
        if (status == ChallengeStatus.Locked)
        {
            status = ChallengeStatus.Unlocked;
        }
    }
    
    /// <summary>
    /// デバッグ用の情報取得
    /// </summary>
    public string GetDebugInfo()
    {
        return $"[{challengeId}] {challengeName}\n" +
               $"難易度: {difficulty}, 種類: {type}\n" +
               $"状態: {status}, 挑戦: {attemptCount}回, クリア: {completedCount}回\n" +
               $"問題: {questionText}\n" +
               $"正解パターン数: {correctAnswers.Length}";
    }
}
