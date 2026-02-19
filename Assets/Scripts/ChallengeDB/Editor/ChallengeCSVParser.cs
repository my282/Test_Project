using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChallengeDB.Editor
{
    /// <summary>
    /// CSV行からChallengeDataを生成するパーサー
    /// </summary>
    public static class ChallengeCSVParser
    {
        /// <summary>
        /// RewardItemsのラッパークラス（JsonUtility用）
        /// </summary>
        [Serializable]
        private class RewardItemsWrapper
        {
            public List<RewardItem> items;
        }

        /// <summary>
        /// CSV行からChallengeDataを作成
        /// </summary>
        /// <param name="row">CSVの1行（Dictionary形式）</param>
        /// <param name="error">エラーメッセージ（エラー時）</param>
        /// <returns>作成されたChallengeData（エラー時はnull）</returns>
        public static ChallengeData CreateChallengeDataFromRow(Dictionary<string, string> row, out string error)
        {
            error = null;
            List<string> errors = new List<string>();

            try
            {
                // ChallengeData作成
                ChallengeData data = ScriptableObject.CreateInstance<ChallengeData>();

                // 必須フィールド: challengeId
                if (!row.TryGetValue("challengeId", out string challengeId) || string.IsNullOrWhiteSpace(challengeId))
                {
                    errors.Add("challengeIdが空です");
                }
                else
                {
                    data.challengeId = challengeId.Trim();
                }

                // 必須フィールド: challengeName
                if (!row.TryGetValue("challengeName", out string challengeName) || string.IsNullOrWhiteSpace(challengeName))
                {
                    errors.Add("challengeNameが空です");
                }
                else
                {
                    data.challengeName = challengeName.Trim();
                }

                // 任意フィールド: description
                if (row.TryGetValue("description", out string description))
                {
                    data.description = description;
                }

                // 任意フィールド: difficulty
                if (row.TryGetValue("difficulty", out string difficultyStr) && !string.IsNullOrWhiteSpace(difficultyStr))
                {
                    if (!ParseEnum(difficultyStr, out ChallengeDifficulty difficulty))
                    {
                        errors.Add($"不正なdifficulty値: {difficultyStr}");
                    }
                    else
                    {
                        data.difficulty = difficulty;
                    }
                }

                // 任意フィールド: type
                if (row.TryGetValue("type", out string typeStr) && !string.IsNullOrWhiteSpace(typeStr))
                {
                    if (!ParseEnum(typeStr, out ChallengeType type))
                    {
                        errors.Add($"不正なtype値: {typeStr}");
                    }
                    else
                    {
                        data.type = type;
                    }
                }

                // 必須フィールド: questionText
                if (!row.TryGetValue("questionText", out string questionText) || string.IsNullOrWhiteSpace(questionText))
                {
                    errors.Add("questionTextが空です");
                }
                else
                {
                    data.questionText = questionText;
                }

                // 任意フィールド: answerType
                if (row.TryGetValue("answerType", out string answerTypeStr) && !string.IsNullOrWhiteSpace(answerTypeStr))
                {
                    if (!ParseEnum(answerTypeStr, out AnswerType answerType))
                    {
                        errors.Add($"不正なanswerType値: {answerTypeStr}");
                    }
                    else
                    {
                        data.answerType = answerType;
                    }
                }

                // 必須フィールド: correctAnswers
                if (!row.TryGetValue("correctAnswers", out string correctAnswersStr) || string.IsNullOrWhiteSpace(correctAnswersStr))
                {
                    errors.Add("correctAnswersが空です");
                }
                else
                {
                    data.correctAnswers = ParseCorrectAnswers(correctAnswersStr);
                    if (data.correctAnswers.Length == 0)
                    {
                        errors.Add("correctAnswersが空の配列です");
                    }
                }

                // 任意フィールド: hint
                if (row.TryGetValue("hint", out string hint))
                {
                    data.hint = hint;
                }

                // 任意フィールド: rewardItems（JSON形式）
                if (row.TryGetValue("rewardItems", out string rewardItemsStr) && !string.IsNullOrWhiteSpace(rewardItemsStr))
                {
                    try
                    {
                        List<RewardItem> rewardItems = ParseRewardItems(rewardItemsStr);
                        data.rewardTable.rewardItems = rewardItems;
                    }
                    catch (Exception e)
                    {
                        errors.Add($"rewardItemsのJSON解析エラー: {e.Message}");
                    }
                }

                // エラーがあれば返却
                if (errors.Count > 0)
                {
                    error = string.Join("\n", errors);
                    return null;
                }

                // バリデーション実行
                if (!data.Validate(out string validationError))
                {
                    error = $"バリデーションエラー:\n{validationError}";
                    return null;
                }

                return data;
            }
            catch (Exception e)
            {
                error = $"予期しないエラー: {e.Message}\n{e.StackTrace}";
                return null;
            }
        }

        /// <summary>
        /// JSON文字列からRewardItemのリストを解析
        /// </summary>
        /// <param name="json">JSON配列文字列</param>
        /// <returns>RewardItemのリスト</returns>
        public static List<RewardItem> ParseRewardItems(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
            {
                return new List<RewardItem>();
            }

            try
            {
                // JSON配列をラップして解析
                string wrappedJson = $"{{\"items\":{json}}}";
                RewardItemsWrapper wrapper = JsonUtility.FromJson<RewardItemsWrapper>(wrappedJson);
                return wrapper.items ?? new List<RewardItem>();
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Invalid RewardItems JSON: {json}", e);
            }
        }

        /// <summary>
        /// パイプ区切りの文字列を配列に変換
        /// </summary>
        /// <param name="value">パイプ区切り文字列</param>
        /// <returns>文字列配列</returns>
        public static string[] ParseCorrectAnswers(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new string[0];
            }

            string[] answers = value.Split('|');
            List<string> result = new List<string>();

            foreach (string answer in answers)
            {
                string trimmed = answer.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    result.Add(trimmed);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 文字列をEnum値に変換
        /// </summary>
        /// <typeparam name="T">Enum型</typeparam>
        /// <param name="value">文字列値</param>
        /// <param name="result">変換結果</param>
        /// <returns>成功したかどうか</returns>
        public static bool ParseEnum<T>(string value, out T result) where T : struct, Enum
        {
            return Enum.TryParse(value.Trim(), ignoreCase: true, out result);
        }
    }
}
