#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// ChallengeDataのカスタムインスペクター
/// 問題作成を効率化する機能を提供
/// </summary>
[CustomEditor(typeof(ChallengeData))]
public class ChallengeDataEditor : Editor
{
    private bool showPreview = true;
    private bool showRewardInfo = true;
    private bool showValidation = true;
    
    private string validationMessage = "";
    private MessageType validationMessageType = MessageType.None;

    public override void OnInspectorGUI()
    {
        ChallengeData challengeData = (ChallengeData)target;
        
        // デフォルトのインスペクター表示
        DrawDefaultInspector();
        
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(10);
        
        // === クイック操作ボタン ===
        EditorGUILayout.LabelField("クイック操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // ID自動生成ボタン
        if (GUILayout.Button("ID自動生成", GUILayout.Height(25)))
        {
            GenerateId(challengeData);
        }
        
        // バリデーション実行ボタン
        if (GUILayout.Button("バリデーション", GUILayout.Height(25)))
        {
            ValidateData(challengeData);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        // 複製して新規作成ボタン
        if (GUILayout.Button("複製して新規作成", GUILayout.Height(25)))
        {
            DuplicateChallenge(challengeData);
        }
        
        // MasterDatabaseに追加ボタン
        if (GUILayout.Button("MasterDBに追加", GUILayout.Height(25)))
        {
            AddToMasterDatabase(challengeData);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // === バリデーション結果 ===
        if (showValidation && !string.IsNullOrEmpty(validationMessage))
        {
            EditorGUILayout.HelpBox(validationMessage, validationMessageType);
            EditorGUILayout.Space(5);
        }
        
        // === プレビューセクション ===
        showPreview = EditorGUILayout.Foldout(showPreview, "問題プレビュー", true, EditorStyles.foldoutHeader);
        if (showPreview)
        {
            DrawPreview(challengeData);
        }
        
        EditorGUILayout.Space(5);
        
        // === 報酬情報セクション ===
        showRewardInfo = EditorGUILayout.Foldout(showRewardInfo, "報酬情報", true, EditorStyles.foldoutHeader);
        if (showRewardInfo)
        {
            DrawRewardInfo(challengeData);
        }
    }
    
    /// <summary>
    /// ID自動生成
    /// </summary>
    private void GenerateId(ChallengeData challengeData)
    {
        string typeStr = challengeData.type.ToString().ToLower();
        string diffStr = challengeData.difficulty.ToString().ToLower();
        
        // 既存のChallengeDataの数を取得して連番を決定
        string[] guids = AssetDatabase.FindAssets("t:ChallengeData");
        int count = guids.Length + 1;
        
        string newId = $"challenge_{typeStr}_{diffStr}_{count:000}";
        
        Undo.RecordObject(challengeData, "Generate Challenge ID");
        challengeData.challengeId = newId;
        EditorUtility.SetDirty(challengeData);
        
        Debug.Log($"ID自動生成: {newId}");
        validationMessage = $"IDを生成しました: {newId}";
        validationMessageType = MessageType.Info;
    }
    
    /// <summary>
    /// データのバリデーション
    /// </summary>
    private void ValidateData(ChallengeData challengeData)
    {
        if (challengeData.Validate(out string errorMessage))
        {
            validationMessage = "✓ すべてのデータが有効です！";
            validationMessageType = MessageType.Info;
            Debug.Log($"[{challengeData.challengeName}] バリデーション成功");
        }
        else
        {
            validationMessage = $"✗ エラー: {errorMessage}";
            validationMessageType = MessageType.Error;
            Debug.LogError($"[{challengeData.challengeName}] {errorMessage}");
        }
        
        Repaint();
    }
    
    /// <summary>
    /// 問題データを複製
    /// </summary>
    private void DuplicateChallenge(ChallengeData original)
    {
        string path = AssetDatabase.GetAssetPath(original);
        string directory = System.IO.Path.GetDirectoryName(path);
        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        string extension = System.IO.Path.GetExtension(path);
        
        // 連番を見つける
        int copyNumber = 1;
        string newPath;
        do
        {
            newPath = $"{directory}/{fileName}_copy{copyNumber}{extension}";
            copyNumber++;
        } while (AssetDatabase.LoadAssetAtPath<ChallengeData>(newPath) != null);
        
        // アセットを複製
        AssetDatabase.CopyAsset(path, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 複製したアセットを取得してIDを更新
        ChallengeData newChallenge = AssetDatabase.LoadAssetAtPath<ChallengeData>(newPath);
        if (newChallenge != null)
        {
            newChallenge.challengeId += $"_copy{copyNumber - 1}";
            newChallenge.challengeName += $" (コピー{copyNumber - 1})";
            EditorUtility.SetDirty(newChallenge);
            AssetDatabase.SaveAssets();
            
            // 新しいアセットを選択
            Selection.activeObject = newChallenge;
            EditorGUIUtility.PingObject(newChallenge);
            
            Debug.Log($"問題を複製しました: {newPath}");
            validationMessage = "問題を複製しました！新しい問題が選択されています。";
            validationMessageType = MessageType.Info;
        }
    }
    
    /// <summary>
    /// MasterDatabaseに追加
    /// </summary>
    private void AddToMasterDatabase(ChallengeData challengeData)
    {
        ChallengeMasterDatabase masterDB = ChallengeMasterDatabase.Instance;
        
        if (masterDB != null)
        {
            masterDB.AddChallengeData(challengeData);
            EditorUtility.SetDirty(masterDB);
            AssetDatabase.SaveAssets();
            
            validationMessage = "ChallengeMasterDatabaseに追加しました！";
            validationMessageType = MessageType.Info;
            Debug.Log($"問題「{challengeData.challengeName}」をChallengeMasterDatabaseに追加しました。");
        }
        else
        {
            validationMessage = "ChallengeMasterDatabaseが見つかりません。Resourcesフォルダに配置してください。";
            validationMessageType = MessageType.Error;
            Debug.LogError("ChallengeMasterDatabaseが見つかりません。Assets/Resources/ChallengeMasterDatabase.asset を作成してください。");
        }
    }
    
    /// <summary>
    /// 問題プレビューを描画
    /// </summary>
    private void DrawPreview(ChallengeData challengeData)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 基本情報
        EditorGUILayout.LabelField("基本情報", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("ID:", challengeData.challengeId);
        EditorGUILayout.LabelField("名前:", challengeData.challengeName);
        EditorGUILayout.LabelField("難易度:", $"{challengeData.difficulty} ({GetDifficultyEmoji(challengeData.difficulty)})");
        EditorGUILayout.LabelField("種類:", challengeData.type.ToString());
        
        EditorGUILayout.Space(5);
        
        // 問題内容
        EditorGUILayout.LabelField("問題文", EditorStyles.miniBoldLabel);
        EditorGUILayout.TextArea(challengeData.questionText, EditorStyles.wordWrappedLabel);
        
        EditorGUILayout.Space(5);
        
        // 回答設定
        EditorGUILayout.LabelField("回答設定", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("入力タイプ:", challengeData.answerType.ToString());
        
        if (challengeData.correctAnswers != null && challengeData.correctAnswers.Length > 0)
        {
            EditorGUILayout.LabelField("正解パターン:", $"{challengeData.correctAnswers.Length}個");
            foreach (var answer in challengeData.correctAnswers)
            {
                EditorGUILayout.LabelField("  •", answer, EditorStyles.wordWrappedLabel);
            }
        }
        else
        {
            EditorGUILayout.LabelField("正解パターン:", "未設定", EditorStyles.boldLabel);
        }
        
        if (challengeData.answerType == AnswerType.Text)
        {
            EditorGUILayout.LabelField("大小文字区別:", challengeData.caseSensitive ? "する" : "しない");
        }
        else if (challengeData.answerType == AnswerType.Number)
        {
            EditorGUILayout.LabelField("許容誤差:", challengeData.numericTolerance.ToString());
        }
        
        // ヒント
        if (!string.IsNullOrEmpty(challengeData.hint))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("ヒント", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(challengeData.hint, EditorStyles.wordWrappedLabel);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 報酬情報を描画
    /// </summary>
    private void DrawRewardInfo(ChallengeData challengeData)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        RewardTable rewardTable = challengeData.rewardTable;
        
        // お金の報酬
        if (rewardTable.guaranteedMoney > 0)
        {
            EditorGUILayout.LabelField("確定報酬（お金）:", $"{rewardTable.guaranteedMoney}円", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.LabelField("確定報酬（お金）:", "なし");
        }
        
        EditorGUILayout.Space(5);
        
        // アイテム報酬
        if (rewardTable.rewardItems != null && rewardTable.rewardItems.Count > 0)
        {
            EditorGUILayout.LabelField("アイテム報酬:", EditorStyles.miniBoldLabel);
            
            float totalDropRate = 0f;
            float expectedValue = 0f;
            
            foreach (var rewardItem in rewardTable.rewardItems)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("  •", GUILayout.Width(20));
                EditorGUILayout.LabelField(
                    $"{rewardItem.itemId}",
                    GUILayout.Width(150)
                );
                EditorGUILayout.LabelField(
                    $"{rewardItem.minQuantity}～{rewardItem.maxQuantity}個",
                    GUILayout.Width(80)
                );
                EditorGUILayout.LabelField(
                    $"{rewardItem.dropRate:P0}",
                    GUILayout.Width(50)
                );
                EditorGUILayout.EndHorizontal();
                
                totalDropRate += rewardItem.dropRate;
                float avgQty = (rewardItem.minQuantity + rewardItem.maxQuantity) / 2f;
                expectedValue += avgQty * rewardItem.dropRate;
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("統計情報", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("ドロップ率合計:", $"{totalDropRate:P1}");
            EditorGUILayout.LabelField("期待値（個数）:", $"{expectedValue:F2}個");
            
            // ドロップ率の警告
            if (totalDropRate > 2.0f)
            {
                EditorGUILayout.HelpBox(
                    "ドロップ率の合計が高いです。報酬が多すぎる可能性があります。",
                    MessageType.Warning
                );
            }
        }
        else
        {
            EditorGUILayout.LabelField("アイテム報酬:", "なし");
        }
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 難易度に応じた絵文字を取得
    /// </summary>
    private string GetDifficultyEmoji(ChallengeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChallengeDifficulty.VeryEasy:
                return "⭐";
            case ChallengeDifficulty.Easy:
                return "⭐⭐";
            case ChallengeDifficulty.Normal:
                return "⭐⭐⭐";
            case ChallengeDifficulty.Hard:
                return "⭐⭐⭐⭐";
            case ChallengeDifficulty.VeryHard:
                return "⭐⭐⭐⭐⭐";
            default:
                return "";
        }
    }
}
#endif
