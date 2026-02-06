using UnityEngine;
using UnityEditor;

/// <summary>
/// MasterDatabaseのカスタムエディタ
/// </summary>
[CustomEditor(typeof(MasterDatabase))]
public class MasterDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MasterDatabase database = (MasterDatabase)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("データベース管理", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("統計情報を表示", GUILayout.Height(30)))
        {
            database.ShowStats();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "アイテムや設備を追加するには:\n" +
            "1. Project > 右クリック > Create > Game > Item Data または Facility Data\n" +
            "2. 作成したアセットをこのリストにドラッグ&ドロップ",
            MessageType.Info
        );
    }
}
