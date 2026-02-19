using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ChallengeDB.Editor
{
    /// <summary>
    /// CSVファイルからChallengeDataを一括インポートするEditorWindow
    /// </summary>
    public class ChallengeCSVImporter : EditorWindow
    {
        private TextAsset csvFile;
        private string outputFolder = "Assets/GameData/Challenges";
        private string statusMessage = "待機中";
        private Vector2 scrollPosition;

        [MenuItem("Tools/Challenge DB/CSV Importer")]
        private static void ShowWindow()
        {
            ChallengeCSVImporter window = GetWindow<ChallengeCSVImporter>("CSV Importer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Challenge CSV Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // CSVファイル選択
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CSVファイル:", GUILayout.Width(100));
            csvFile = (TextAsset)EditorGUILayout.ObjectField(csvFile, typeof(TextAsset), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 出力先フォルダ
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("出力先:", GUILayout.Width(100));
            outputFolder = EditorGUILayout.TextField(outputFolder);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 説明
            EditorGUILayout.HelpBox(
                "CSVファイルを選択して「インポート実行」を押してください。\n" +
                "エラーが発生した場合は即座に中断されます。\n\n" +
                "フォルダ構造: {出力先}/{type}/{difficulty}/xxx.asset",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // インポートボタン
            GUI.enabled = csvFile != null;
            if (GUILayout.Button("インポート実行", GUILayout.Height(40)))
            {
                ExecuteImport();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // ステータス表示
            EditorGUILayout.LabelField("ステータス:");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
            EditorGUILayout.TextArea(statusMessage, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// CSVインポートを実行
        /// </summary>
        private void ExecuteImport()
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("エラー", "CSVファイルが選択されていません。", "OK");
                return;
            }

            // ChallengeMasterDatabase確認
            ChallengeMasterDatabase masterDB = ChallengeMasterDatabase.Instance;
            if (masterDB == null)
            {
                EditorUtility.DisplayDialog("エラー",
                    "ChallengeMasterDatabaseが見つかりません。\n" +
                    "Assets/Resources/ChallengeMasterDatabase.asset を作成してください。",
                    "OK");
                return;
            }

            try
            {
                statusMessage = "CSVファイルを読み込み中...";
                Repaint();

                // CSV読み込み
                string csvText = csvFile.text;
                
                statusMessage = "CSVを解析中...";
                Repaint();

                // CSVパース
                List<Dictionary<string, string>> rows;
                try
                {
                    rows = CSVSimpleParser.ParseCSV(csvText);
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("CSV構文エラー",
                        $"CSVファイルの解析に失敗しました。\n\n{e.Message}",
                        "OK");
                    statusMessage = $"エラー: {e.Message}";
                    return;
                }

                if (rows.Count == 0)
                {
                    EditorUtility.DisplayDialog("エラー", "CSVにデータ行がありません。", "OK");
                    statusMessage = "エラー: データ行なし";
                    return;
                }

                statusMessage = $"{rows.Count}件のデータを検出しました。インポート開始...";
                Repaint();

                // 出力フォルダ確認
                if (!AssetDatabase.IsValidFolder(outputFolder))
                {
                    EnsureFolderExists(outputFolder);
                }

                // インポート処理
                Stopwatch stopwatch = Stopwatch.StartNew();
                int successCount = 0;
                int totalCount = rows.Count;

                for (int i = 0; i < rows.Count; i++)
                {
                    float progress = (float)i / rows.Count;
                    Dictionary<string, string> row = rows[i];

                    // キャンセル可能なプログレスバー
                    string rowId = row.ContainsKey("challengeId") ? row["challengeId"] : $"Row {i + 1}";
                    bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                        "CSV Import",
                        $"Importing {i + 1}/{rows.Count}: {rowId}",
                        progress
                    );

                    if (cancelled)
                    {
                        EditorUtility.ClearProgressBar();
                        int imported = successCount;
                        EditorUtility.DisplayDialog("キャンセル",
                            $"{imported}問インポートした時点でキャンセルされました。",
                            "OK");
                        statusMessage = $"キャンセル: {imported}件インポート済み";
                        return;
                    }

                    // ChallengeData作成
                    ChallengeData data = ChallengeCSVParser.CreateChallengeDataFromRow(row, out string error);
                    if (data == null)
                    {
                        // エラー発生 → 即座に中断
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("インポートエラー",
                            $"Row {i + 1}: {rowId}\n\nエラー内容:\n{error}\n\nインポートを中断しました。",
                            "OK");
                        statusMessage = $"エラー (Row {i + 1}): {error}";
                        return;
                    }

                    // アセット保存
                    string folderPath = $"{outputFolder}/{data.type}/{data.difficulty}";
                    EnsureFolderExists(folderPath);

                    string assetPath = $"{folderPath}/{data.challengeId}.asset";
                    
                    // 既存アセットがあれば警告（上書きはしない）
                    if (File.Exists(assetPath))
                    {
                        EditorUtility.ClearProgressBar();
                        bool overwrite = EditorUtility.DisplayDialog("ファイル既存",
                            $"既に {data.challengeId}.asset が存在します。\n上書きしますか？",
                            "上書き", "キャンセル");
                        
                        if (!overwrite)
                        {
                            statusMessage = $"キャンセル: {successCount}件インポート済み（{data.challengeId}で中断）";
                            return;
                        }
                    }

                    AssetDatabase.CreateAsset(data, assetPath);

                    // MasterDBに登録
                    masterDB.AddChallengeData(data);

                    successCount++;

                    // 定期的に保存
                    if (i % 100 == 0 && i > 0)
                    {
                        EditorUtility.SetDirty(masterDB);
                        AssetDatabase.SaveAssets();
                    }
                }

                // 最終保存
                EditorUtility.SetDirty(masterDB);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                stopwatch.Stop();

                // 完了ダイアログ
                EditorUtility.DisplayDialog("インポート完了",
                    $"成功: {successCount}件 / {totalCount}件\n所要時間: {stopwatch.Elapsed.TotalSeconds:F2}秒",
                    "OK");

                statusMessage = $"完了: {successCount}件インポート成功（{stopwatch.Elapsed.TotalSeconds:F2}秒）";

                // ChallengeMasterDatabaseを選択して統計を確認しやすくする
                Selection.activeObject = masterDB;
                EditorGUIUtility.PingObject(masterDB);
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("予期しないエラー",
                    $"インポート中に予期しないエラーが発生しました。\n\n{e.Message}\n\n{e.StackTrace}",
                    "OK");
                statusMessage = $"予期しないエラー: {e.Message}";
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// フォルダが存在しない場合は作成
        /// </summary>
        private static void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0]; // "Assets"

            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = nextPath;
            }
        }
    }
}
