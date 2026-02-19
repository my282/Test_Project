using System;

/// <summary>
/// 問題の難易度（5段階）
/// </summary>
public enum ChallengeDifficulty
{
    VeryEasy,   // 非常に簡単
    Easy,       // 簡単
    Normal,     // 普通
    Hard,       // 難しい
    VeryHard    // 非常に難しい
}

/// <summary>
/// 問題の種類
/// </summary>
public enum ChallengeType
{
    Math,       // 数学
    Logic,      // 論理
    Memory,     // 記憶
    Quiz,       // クイズ
    Other       // その他
}

/// <summary>
/// 回答の入力タイプ
/// </summary>
public enum AnswerType
{
    Number,     // 数値入力
    Text        // テキスト入力
}

/// <summary>
/// 問題の状態
/// </summary>
public enum ChallengeStatus
{
    Locked,     // ロック中（未解放）
    Unlocked,   // 解放済み
    Completed   // クリア済み
}
