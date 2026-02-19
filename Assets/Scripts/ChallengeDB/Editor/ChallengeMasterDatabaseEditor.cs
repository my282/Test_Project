#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// ChallengeMasterDatabaseのカスタムインスペクター
/// </summary>
[CustomEditor(typeof(ChallengeMasterDatabase))]
public class ChallengeMasterDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ChallengeMasterDatabase database = (ChallengeMasterDatabase)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(10);

        // === 統計情報 ===
        EditorGUILayout.LabelField("統計情報", EditorStyles.boldLabel);

        if (GUILayout.Button("統計を表示", GUILayout.Height(30)))
        {
            string stats = database.GetStatistics();
            Debug.Log(stats);
            EditorUtility.DisplayDialog("統計情報", stats, "OK");
        }

        EditorGUILayout.Space(10);

        // === データ管理 ===
        EditorGUILayout.LabelField("データ管理", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("すべて削除", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog(
                "確認",
                "本当にすべての問題データを削除しますか？\nこの操作は取り消せません！",
                "削除",
                "キャンセル"))
            {
                database.ClearAllChallenges();
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }
        }

        if (GUILayout.Button("保存", GUILayout.Height(25)))
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log("ChallengeMasterDatabaseを保存しました。");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // === クイック情報 ===
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("クイック情報", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("総問題数:", database.GetChallengeCount().ToString());
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("難易度別:", EditorStyles.miniBoldLabel);
        foreach (ChallengeDifficulty diff in System.Enum.GetValues(typeof(ChallengeDifficulty)))
        {
            int count = database.GetChallengeCountByDifficulty(diff);
            EditorGUILayout.LabelField($"  {diff}:", $"{count}問");
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("種類別:", EditorStyles.miniBoldLabel);
        foreach (ChallengeType type in System.Enum.GetValues(typeof(ChallengeType)))
        {
            int count = database.GetChallengeCountByType(type);
            EditorGUILayout.LabelField($"  {type}:", $"{count}問");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // === 使い方ガイド ===
        EditorGUILayout.HelpBox(
            "【使い方】\n" +
            "1. ChallengeDataを作成\n" +
            "2. Inspector下部の「MasterDBに追加」ボタンをクリック\n" +
            "3. このChallengeMasterDatabaseに自動登録されます",
            MessageType.Info
        );
    }
}
#endif
