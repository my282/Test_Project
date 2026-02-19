using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// チャレンジシーンのUI表示とインタラクションを管理
/// </summary>
public class ChallengeUIManager : MonoBehaviour
{
    [Header("問題表示")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text difficultyText;
    
    [Header("進捗表示")]
    [SerializeField] private TMP_Text progressText;
    
    [Header("回答入力")]
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text submitButtonText;
    
    [Header("ヒント表示")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Button showHintButton;
    
    [Header("結果表示")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultMessageText;
    [SerializeField] private TMP_Text correctAnswerText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Image resultIcon;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite incorrectSprite;
    
    [Header("完了画面")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionMessageText;
    [SerializeField] private TMP_Text statisticsText;
    [SerializeField] private Button backToHubButton;
    
    [Header("色設定")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    
    private bool isHintShown = false;
    
    private void Start()
    {
        // 初期状態設定
        if (resultPanel != null) resultPanel.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        
        // ボタンイベント設定
        if (showHintButton != null)
        {
            showHintButton.onClick.AddListener(OnShowHintClicked);
        }
    }
    
    /// <summary>
    /// 問題を表示
    /// </summary>
    /// <param name="challenge">表示する問題</param>
    /// <param name="currentIndex">現在の問題番号（1始まり）</param>
    /// <param name="totalCount">総問題数</param>
    public void ShowQuestion(Challenge challenge, int currentIndex, int totalCount)
    {
        if (challenge == null)
        {
            Debug.LogError("ChallengeUIManager: challengeがnullです");
            return;
        }
        
        // 問題文表示
        if (questionText != null)
        {
            questionText.text = challenge.questionText;
        }
        
        // 問題タイプ表示
        if (typeText != null)
        {
            typeText.text = GetTypeDisplayName(challenge.type);
        }
        
        // 難易度表示
        if (difficultyText != null)
        {
            difficultyText.text = GetDifficultyDisplayName(challenge.difficulty);
        }
        
        // 進捗表示
        if (progressText != null)
        {
            progressText.text = $"{currentIndex} / {totalCount}";
        }
        
        // ヒント設定
        if (hintText != null && !string.IsNullOrEmpty(challenge.hint))
        {
            hintText.text = challenge.hint;
            if (showHintButton != null)
            {
                showHintButton.gameObject.SetActive(true);
            }
        }
        else
        {
            if (showHintButton != null)
            {
                showHintButton.gameObject.SetActive(false);
            }
        }
        
        // 入力フィールドプレースホルダー設定
        if (answerInputField != null)
        {
            var placeholder = answerInputField.placeholder as TMP_Text;
            if (placeholder != null)
            {
                placeholder.text = challenge.answerType == AnswerType.Number 
                    ? "数値を入力してください" 
                    : "答えを入力してください";
            }
            
            // キーボードタイプ設定
            answerInputField.contentType = challenge.answerType == AnswerType.Number 
                ? TMP_InputField.ContentType.DecimalNumber 
                : TMP_InputField.ContentType.Standard;
        }
        
        // ヒント非表示にリセット
        isHintShown = false;
        if (hintPanel != null) hintPanel.SetActive(false);
        
        Debug.Log($"問題表示: {challenge.challengeName} ({currentIndex}/{totalCount})");
    }
    
    /// <summary>
    /// 結果を表示（正解/不正解）
    /// </summary>
    /// <param name="isCorrect">正解かどうか</param>
    /// <param name="correctAnswer">正解の答え</param>
    /// <param name="userAnswer">ユーザーの回答</param>
    public void ShowResult(bool isCorrect, string correctAnswer, string userAnswer)
    {
        if (resultPanel == null)
        {
            Debug.LogWarning("ChallengeUIManager: resultPanelが設定されていません");
            return;
        }
        
        resultPanel.SetActive(true);
        
        // 結果メッセージ
        if (resultMessageText != null)
        {
            if (isCorrect)
            {
                resultMessageText.text = "正解！";
                resultMessageText.color = correctColor;
            }
            else
            {
                resultMessageText.text = "不正解...";
                resultMessageText.color = incorrectColor;
            }
        }
        
        // 正解表示
        if (correctAnswerText != null)
        {
            if (isCorrect)
            {
                correctAnswerText.text = $"あなたの答え: {userAnswer}";
            }
            else
            {
                correctAnswerText.text = $"正解: {correctAnswer}\nあなたの答え: {userAnswer}";
            }
        }
        
        // 結果アイコン
        if (resultIcon != null)
        {
            resultIcon.sprite = isCorrect ? correctSprite : incorrectSprite;
            resultIcon.color = isCorrect ? correctColor : incorrectColor;
        }
        
        // 送信ボタン無効化
        if (submitButton != null)
        {
            submitButton.interactable = false;
        }
        
        // 入力フィールド無効化
        if (answerInputField != null)
        {
            answerInputField.interactable = false;
        }
    }
    
    /// <summary>
    /// 完了画面を表示
    /// </summary>
    /// <param name="solvedCount">解決した問題数</param>
    /// <param name="difficulty">対象難易度</param>
    public void ShowCompletion(int solvedCount, ChallengeDifficulty difficulty)
    {
        if (completionPanel == null)
        {
            Debug.LogWarning("ChallengeUIManager: completionPanelが設定されていません");
            return;
        }
        
        completionPanel.SetActive(true);
        
        // 完了メッセージ
        if (completionMessageText != null)
        {
            completionMessageText.text = $"{GetDifficultyDisplayName(difficulty)}の問題を\nすべてクリアしました！";
        }
        
        // 統計情報
        if (statisticsText != null)
        {
            statisticsText.text = $"クリアした問題数: {solvedCount}問";
        }
        
        Debug.Log($"{difficulty}難易度の全問題完了");
    }
    
    /// <summary>
    /// 次の問題用にUIをリセット
    /// </summary>
    public void ResetForNextQuestion()
    {
        // 結果パネル非表示
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 入力フィールドクリア&有効化
        if (answerInputField != null)
        {
            answerInputField.text = "";
            answerInputField.interactable = true;
            answerInputField.ActivateInputField();
        }
        
        // 送信ボタン有効化
        if (submitButton != null)
        {
            submitButton.interactable = true;
        }
        
        // ヒントパネル非表示
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
        
        isHintShown = false;
    }
    
    /// <summary>
    /// 入力フィールドの値を取得
    /// </summary>
    /// <returns>ユーザー入力</returns>
    public string GetUserAnswer()
    {
        if (answerInputField != null)
        {
            return answerInputField.text.Trim();
        }
        
        return "";
    }
    
    /// <summary>
    /// 送信ボタンにリスナーを追加
    /// </summary>
    public void SetSubmitButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(action);
        }
    }
    
    /// <summary>
    /// 次へボタンにリスナーを追加
    /// </summary>
    public void SetNextButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(action);
        }
    }
    
    /// <summary>
    /// ハブに戻るボタンにリスナーを追加
    /// </summary>
    public void SetBackToHubButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (backToHubButton != null)
        {
            backToHubButton.onClick.RemoveAllListeners();
            backToHubButton.onClick.AddListener(action);
        }
    }
    
    /// <summary>
    /// ヒント表示ボタンがクリックされた時
    /// </summary>
    private void OnShowHintClicked()
    {
        if (hintPanel != null)
        {
            isHintShown = !isHintShown;
            hintPanel.SetActive(isHintShown);
        }
    }
    
    /// <summary>
    /// 問題タイプの表示名を取得
    /// </summary>
    private string GetTypeDisplayName(ChallengeType type)
    {
        switch (type)
        {
            case ChallengeType.Math: return "数学";
            case ChallengeType.Logic: return "論理";
            case ChallengeType.Memory: return "記憶";
            case ChallengeType.Quiz: return "クイズ";
            case ChallengeType.Other: return "その他";
            default: return type.ToString();
        }
    }
    
    /// <summary>
    /// 難易度の表示名を取得
    /// </summary>
    private string GetDifficultyDisplayName(ChallengeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChallengeDifficulty.Beginner: return "初心者";
            case ChallengeDifficulty.Easy: return "簡単";
            case ChallengeDifficulty.Normal: return "普通";
            case ChallengeDifficulty.Hard: return "難しい";
            case ChallengeDifficulty.Expert: return "上級者";
            default: return difficulty.ToString();
        }
    }
}
