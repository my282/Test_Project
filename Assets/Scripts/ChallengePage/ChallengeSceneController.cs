using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// チャレンジシーンのゲームフローを制御
/// </summary>
public class ChallengeSceneController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private ChallengeDifficulty targetDifficulty = ChallengeDifficulty.Easy;
    [SerializeField] private ChallengeUIManager uiManager;
    
    [Header("デバッグ")]
    [SerializeField] private bool enableDebugLog = true;
    
    // 現在のゲーム状態
    private List<Challenge> unsolvedChallenges = new List<Challenge>();
    private Challenge currentChallenge;
    private int currentQuestionIndex = 0;
    private int totalQuestionsCount = 0;
    private int solvedInThisSessionCount = 0;
    
    private void Start()
    {
        // UI マネージャー検証
        if (uiManager == null)
        {
            Debug.LogError("ChallengeSceneController: UIManagerが設定されていません");
            return;
        }
        
        // ChallengeMasterDatabase が初期化されているか確認
        if (ChallengeMasterDatabase.Instance == null)
        {
            Debug.LogError("ChallengeSceneController: ChallengeMasterDatabaseが見つかりません");
            return;
        }
        
        // ボタンイベント設定
        uiManager.SetSubmitButtonListener(OnSubmitAnswer);
        uiManager.SetNextButtonListener(OnNextQuestion);
        uiManager.SetBackToHubButtonListener(OnBackToHub);
        
        // ゲーム開始
        InitializeGame();
    }
    
    /// <summary>
    /// ゲームを初期化して最初の問題を表示
    /// </summary>
    private void InitializeGame()
    {
        DebugLog("=== ゲーム初期化 ===");
        
        // 指定難易度の全問題データを取得
        var allChallengeData = ChallengeMasterDatabase.Instance.GetChallengeDataByDifficulty(targetDifficulty);
        
        if (allChallengeData == null || allChallengeData.Count == 0)
        {
            Debug.LogError($"ChallengeSceneController: {targetDifficulty}難易度の問題が見つかりません");
            ShowNoQuestionsError();
            return;
        }
        
        DebugLog($"{targetDifficulty}難易度の問題数: {allChallengeData.Count}");
        
        // 未解決問題をフィルタリング
        var unsolvedData = ChallengeProgressManager.Instance.GetUnsolvedChallenges(allChallengeData);
        
        if (unsolvedData.Count == 0)
        {
            DebugLog("未解決問題がありません。完了画面を表示します。");
            ShowCompletionScreen();
            return;
        }
        
        DebugLog($"未解決問題数: {unsolvedData.Count}");
        
        // ChallengeDataからChallengeインスタンスを生成
        unsolvedChallenges = unsolvedData.Select(data => data.CreateChallenge()).ToList();
        
        // ランダムシャッフル
        ShuffleChallenges();
        
        totalQuestionsCount = unsolvedChallenges.Count;
        currentQuestionIndex = 0;
        solvedInThisSessionCount = 0;
        
        // 最初の問題を表示
        ShowNextQuestion();
    }
    
    /// <summary>
    /// 次の問題を表示
    /// </summary>
    private void ShowNextQuestion()
    {
        if (currentQuestionIndex >= unsolvedChallenges.Count)
        {
            DebugLog("すべての問題が完了しました");
            ShowCompletionScreen();
            return;
        }
        
        currentChallenge = unsolvedChallenges[currentQuestionIndex];
        
        // UIをリセットして問題表示
        uiManager.ResetForNextQuestion();
        uiManager.ShowQuestion(currentChallenge, currentQuestionIndex + 1, totalQuestionsCount);
        
        DebugLog($"問題表示 ({currentQuestionIndex + 1}/{totalQuestionsCount}): {currentChallenge.challengeName}");
    }
    
    /// <summary>
    /// 回答送信ボタンがクリックされた時
    /// </summary>
    private void OnSubmitAnswer()
    {
        if (currentChallenge == null)
        {
            Debug.LogWarning("ChallengeSceneController: 現在の問題がありません");
            return;
        }
        
        // ユーザーの回答を取得
        string userAnswer = uiManager.GetUserAnswer();
        
        if (string.IsNullOrWhiteSpace(userAnswer))
        {
            Debug.LogWarning("回答が入力されていません");
            return;
        }
        
        DebugLog($"ユーザー回答: {userAnswer}");
        
        // 回答判定
        bool isCorrect = currentChallenge.CheckAnswer(userAnswer);
        
        // 挑戦回数を増やす
        currentChallenge.attemptCount++;
        
        if (isCorrect)
        {
            // 正解時の処理
            currentChallenge.completedCount++;
            currentChallenge.lastCompletedTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentChallenge.status = ChallengeStatus.Completed;
            
            // 回答済みとしてマーク
            ChallengeProgressManager.Instance.MarkAsSolved(currentChallenge.challengeId, currentChallenge.difficulty);
            
            solvedInThisSessionCount++;
            
            DebugLog($"正解！ セッション内解決数: {solvedInThisSessionCount}");
        }
        else
        {
            DebugLog("不正解");
        }
        
        // 正解の文字列を取得（複数ある場合は最初の1つ）
        string correctAnswerDisplay = currentChallenge.correctAnswers != null && currentChallenge.correctAnswers.Length > 0
            ? currentChallenge.correctAnswers[0]
            : "（不明）";
        
        // 結果を表示
        uiManager.ShowResult(isCorrect, correctAnswerDisplay, userAnswer);
    }
    
    /// <summary>
    /// 次へボタンがクリックされた時
    /// </summary>
    private void OnNextQuestion()
    {
        currentQuestionIndex++;
        ShowNextQuestion();
    }
    
    /// <summary>
    /// ハブに戻るボタンがクリックされた時
    /// </summary>
    private void OnBackToHub()
    {
        DebugLog("ハブシーンに戻ります");
        
        // SceneLoaderを使用してフェード付き遷移
        SceneLoader.Load("hub_page");
    }
    
    /// <summary>
    /// 完了画面を表示
    /// </summary>
    private void ShowCompletionScreen()
    {
        int totalSolvedCount = ChallengeProgressManager.Instance.GetSolvedCount(targetDifficulty);
        uiManager.ShowCompletion(totalSolvedCount, targetDifficulty);
        
        DebugLog($"完了画面表示: 総解決数 = {totalSolvedCount}");
    }
    
    /// <summary>
    /// 問題が見つからない場合のエラー表示
    /// </summary>
    private void ShowNoQuestionsError()
    {
        Debug.LogError($"{targetDifficulty}難易度の問題が登録されていません。ChallengeMasterDatabaseを確認してください。");
        
        // エラーメッセージを表示してハブに戻る
        // TODO: エラーダイアログの実装
        Invoke(nameof(OnBackToHub), 2f);
    }
    
    /// <summary>
    /// 問題リストをシャッフル
    /// </summary>
    private void ShuffleChallenges()
    {
        // Fisher-Yates shuffle
        System.Random rng = new System.Random();
        int n = unsolvedChallenges.Count;
        
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Challenge temp = unsolvedChallenges[k];
            unsolvedChallenges[k] = unsolvedChallenges[n];
            unsolvedChallenges[n] = temp;
        }
        
        DebugLog("問題リストをシャッフルしました");
    }
    
    /// <summary>
    /// デバッグログ出力
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[ChallengeSceneController] {message}");
        }
    }
    
    /// <summary>
    /// 現在の進行状況を取得（デバッグ用）
    /// </summary>
    public string GetProgressInfo()
    {
        return $"問題: {currentQuestionIndex + 1}/{totalQuestionsCount}, セッション解決数: {solvedInThisSessionCount}";
    }
    
    #region コンテキストメニュー（デバッグ用）
    
    [ContextMenu("進行状況を表示")]
    private void ShowProgress()
    {
        Debug.Log(GetProgressInfo());
        ChallengeProgressManager.Instance.PrintStatus();
    }
    
    [ContextMenu("履歴をリセット")]
    private void ResetProgress()
    {
        ChallengeProgressManager.Instance.ResetByDifficulty(targetDifficulty);
        InitializeGame();
    }
    
    [ContextMenu("全履歴をリセット")]
    private void ResetAllProgress()
    {
        ChallengeProgressManager.Instance.Reset();
        InitializeGame();
    }
    
    #endregion
}
