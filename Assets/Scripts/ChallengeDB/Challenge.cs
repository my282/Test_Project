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
    public Sprite icon;
    public ChallengeDifficulty difficulty;
    public ChallengeType type;
    
    [Header("問題内容")]
    public string questionText;
    public Sprite questionImage;
    
    [Header("回答設定")]
    public AnswerType answerType;
    public string[] correctAnswers;
    public bool caseSensitive;
    public float numericTolerance;
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
        Sprite iconSprite,
        ChallengeDifficulty diff,
        ChallengeType challengeType,
        string question,
        Sprite questionImg,
        AnswerType ansType,
        string[] answers,
        bool caseSens,
        float numTolerance,
        string hintText
    )
    {
        challengeId = id;
        challengeName = name;
        description = desc;
        icon = iconSprite;
        difficulty = diff;
        type = challengeType;
        questionText = question;
        questionImage = questionImg;
        answerType = ansType;
        correctAnswers = answers;
        caseSensitive = caseSens;
        numericTolerance = numTolerance;
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
    /// 数値回答のチェック
    /// </summary>
    private bool CheckNumericAnswer(string userAnswer)
    {
        // ユーザーの入力を数値に変換
        if (!float.TryParse(userAnswer, out float userValue))
        {
            return false;
        }
        
        // すべての正解パターンと比較
        foreach (var correctAnswer in correctAnswers)
        {
            if (float.TryParse(correctAnswer, out float correctValue))
            {
                // 許容誤差内かチェック
                if (Mathf.Abs(userValue - correctValue) <= numericTolerance)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// テキスト回答のチェック
    /// </summary>
    private bool CheckTextAnswer(string userAnswer)
    {
        // すべての正解パターンと比較
        foreach (var correctAnswer in correctAnswers)
        {
            // 大文字小文字の区別設定に応じて比較
            StringComparison comparison = caseSensitive 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;
            
            if (userAnswer.Trim().Equals(correctAnswer.Trim(), comparison))
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
