using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// HubPageManagerのカスタムエディタ
/// </summary>
[CustomEditor(typeof(HubPageManager))]
public class HubPageManagerEditor : Editor
{
    private SerializedProperty pageButtons;
    private SerializedProperty buttonContainer;
    private SerializedProperty buttonSpacing;
    private SerializedProperty customButtonPrefab;
    
    private bool showLayoutSettings = true;
    private bool showPrefabSettings = true;
    
    private void OnEnable()
    {
        pageButtons = serializedObject.FindProperty("pageButtons");
        buttonContainer = serializedObject.FindProperty("buttonContainer");
        buttonSpacing = serializedObject.FindProperty("buttonSpacing");
        customButtonPrefab = serializedObject.FindProperty("customButtonPrefab");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space(10);
        DrawHeader();
        EditorGUILayout.Space(10);
        
        // ボタンリスト
        DrawButtonList();
        
        EditorGUILayout.Space(10);
        
        // レイアウト設定
        showLayoutSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showLayoutSettings, "レイアウト設定");
        if (showLayoutSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(buttonContainer);
            
            SerializedProperty layoutMode = serializedObject.FindProperty("layoutMode");
            EditorGUILayout.PropertyField(layoutMode, new GUIContent("レイアウトモード"));
            
            // レイアウトモードに応じた説明を表示
            LayoutMode mode = (LayoutMode)layoutMode.enumValueIndex;
            switch (mode)
            {
                case LayoutMode.Vertical:
                    EditorGUILayout.HelpBox("縦方向に自動配置されます。", MessageType.Info);
                    break;
                case LayoutMode.Horizontal:
                    EditorGUILayout.HelpBox("横方向に自動配置されます。", MessageType.Info);
                    break;
                case LayoutMode.Grid:
                    EditorGUILayout.HelpBox("グリッド状に自動配置されます。", MessageType.Info);
                    SerializedProperty gridColumns = serializedObject.FindProperty("gridColumns");
                    SerializedProperty gridCellSize = serializedObject.FindProperty("gridCellSize");
                    EditorGUILayout.PropertyField(gridColumns);
                    EditorGUILayout.PropertyField(gridCellSize);
                    break;
                case LayoutMode.Custom:
                    EditorGUILayout.HelpBox("各ボタンの「カスタム位置を使用」設定に従います。", MessageType.Info);
                    break;
            }
            
            if (mode != LayoutMode.Custom)
            {
                EditorGUILayout.PropertyField(buttonSpacing);
                SerializedProperty startPosition = serializedObject.FindProperty("startPosition");
                EditorGUILayout.PropertyField(startPosition, new GUIContent("開始位置"));
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(5);
        
        // プレハブ設定
        showPrefabSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showPrefabSettings, "プレハブ設定（オプション）");
        if (showPrefabSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(customButtonPrefab);
            EditorGUILayout.HelpBox("カスタムボタンプレハブを設定しない場合は、デフォルトボタンが自動生成されます。", MessageType.Info);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        
        // ボタン操作
        DrawButtonActions();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Hub Page Manager", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("シーン遷移ボタンを管理します。ボタンのデザインや画像を自由にカスタマイズできます。", MessageType.Info);
    }
    
    private void DrawButtonList()
    {
        EditorGUILayout.LabelField("ボタンリスト", EditorStyles.boldLabel);
        
        // ボタン追加
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+ 新規ボタンを追加", GUILayout.Width(150)))
        {
            pageButtons.arraySize++;
            var newButton = pageButtons.GetArrayElementAtIndex(pageButtons.arraySize - 1);
            InitializeNewButton(newButton);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // ボタン一覧表示
        for (int i = 0; i < pageButtons.arraySize; i++)
        {
            DrawButtonElement(i);
        }
        
        if (pageButtons.arraySize == 0)
        {
            EditorGUILayout.HelpBox("ボタンが追加されていません。「新規ボタンを追加」をクリックしてボタンを作成してください。", MessageType.Warning);
        }
    }
    
    private void DrawButtonElement(int index)
    {
        SerializedProperty button = pageButtons.GetArrayElementAtIndex(index);
        SerializedProperty sceneName = button.FindPropertyRelative("sceneName");
        SerializedProperty buttonText = button.FindPropertyRelative("buttonText");
        
        string displayName = string.IsNullOrEmpty(buttonText.stringValue) 
            ? $"ボタン {index + 1}" 
            : buttonText.stringValue;
        
        EditorGUILayout.BeginVertical("box");
        
        // ヘッダー
        EditorGUILayout.BeginHorizontal();
        button.isExpanded = EditorGUILayout.Foldout(button.isExpanded, displayName, true);
        
        GUILayout.FlexibleSpace();
        
        // 削除ボタン
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            if (EditorUtility.DisplayDialog("確認", $"「{displayName}」を削除しますか?", "削除", "キャンセル"))
            {
                pageButtons.DeleteArrayElementAtIndex(index);
                return;
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // 詳細表示
        if (button.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            // ページ設定
            DrawSection("ページ設定", () =>
            {
                EditorGUILayout.PropertyField(sceneName, new GUIContent("シーン名"));
                EditorGUILayout.PropertyField(buttonText, new GUIContent("ボタンテキスト"));
            });
            
            // 位置設定
            DrawSection("位置設定", () =>
            {
                SerializedProperty useCustomPosition = button.FindPropertyRelative("useCustomPosition");
                SerializedProperty customPosition = button.FindPropertyRelative("customPosition");
                SerializedProperty anchorPreset = button.FindPropertyRelative("anchorPreset");
                
                EditorGUILayout.PropertyField(useCustomPosition, new GUIContent("カスタム位置を使用"));
                
                if (useCustomPosition.boolValue)
                {
                    EditorGUILayout.PropertyField(anchorPreset, new GUIContent("アンカー"));
                    EditorGUILayout.PropertyField(customPosition, new GUIContent("位置 (X, Y)"));
                    EditorGUILayout.HelpBox("カスタム位置を使用すると、レイアウトモードの自動配置は無視されます。", MessageType.None);
                }
                else
                {
                    EditorGUILayout.HelpBox("レイアウトモードに従って自動配置されます。", MessageType.Info);
                }
            });
            
            // 画像設定
            DrawSection("画像設定", () =>
            {
                SerializedProperty backgroundSprite = button.FindPropertyRelative("backgroundSprite");
                SerializedProperty imageType = button.FindPropertyRelative("imageType");
                SerializedProperty iconSprite = button.FindPropertyRelative("iconSprite");
                SerializedProperty iconSize = button.FindPropertyRelative("iconSize");
                SerializedProperty iconPosition = button.FindPropertyRelative("iconPosition");
                
                EditorGUILayout.PropertyField(backgroundSprite, new GUIContent("背景画像"));
                if (backgroundSprite.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(imageType, new GUIContent("画像タイプ"));
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(iconSprite, new GUIContent("アイコン画像"));
                if (iconSprite.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(iconSize, new GUIContent("アイコンサイズ"));
                    EditorGUILayout.PropertyField(iconPosition, new GUIContent("アイコン位置"));
                }
            });
            
            // デザイン設定
            DrawSection("デザイン設定", () =>
            {
                EditorGUILayout.PropertyField(button.FindPropertyRelative("backgroundColor"), new GUIContent("背景色"));
                EditorGUILayout.PropertyField(button.FindPropertyRelative("textColor"), new GUIContent("テキスト色"));
                EditorGUILayout.PropertyField(button.FindPropertyRelative("fontSize"), new GUIContent("フォントサイズ"));
                EditorGUILayout.PropertyField(button.FindPropertyRelative("buttonWidth"), new GUIContent("ボタン幅"));
                EditorGUILayout.PropertyField(button.FindPropertyRelative("buttonHeight"), new GUIContent("ボタン高さ"));
            });
            
            // ホバーエフェクト
            DrawSection("ホバーエフェクト", () =>
            {
                SerializedProperty enableHoverEffect = button.FindPropertyRelative("enableHoverEffect");
                EditorGUILayout.PropertyField(enableHoverEffect, new GUIContent("ホバーエフェクトを有効化"));
                
                if (enableHoverEffect.boolValue)
                {
                    EditorGUILayout.PropertyField(button.FindPropertyRelative("hoverColor"), new GUIContent("ホバー時の色"));
                    EditorGUILayout.PropertyField(button.FindPropertyRelative("pressedColor"), new GUIContent("クリック時の色"));
                }
            });
            
            // 枠線設定
            DrawSection("枠線設定", () =>
            {
                SerializedProperty showBorder = button.FindPropertyRelative("showBorder");
                EditorGUILayout.PropertyField(showBorder, new GUIContent("枠線を表示"));
                
                if (showBorder.boolValue)
                {
                    EditorGUILayout.PropertyField(button.FindPropertyRelative("borderColor"), new GUIContent("枠線の色"));
                    EditorGUILayout.PropertyField(button.FindPropertyRelative("borderWidth"), new GUIContent("枠線の太さ"));
                }
            });
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void DrawSection(string title, System.Action drawContent)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        drawContent?.Invoke();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }
    
    private void DrawButtonActions()
    {
        EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.5f, 1f, 0.8f);
        if (GUILayout.Button("ボタンをプレビュー", GUILayout.Height(35)))
        {
            HubPageManager manager = (HubPageManager)target;
            manager.RegenerateButtons();
            EditorUtility.SetDirty(manager);
            Debug.Log("ボタンをプレビューしました。Scene ビューまたは Game ビューで確認してください。");
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("シーン検証", GUILayout.Height(30)))
        {
            ValidateScenes();
        }
        
        if (GUILayout.Button("ビルド設定を開く", GUILayout.Height(30)))
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox("💡 エディタモードでもボタンが表示されます！\n" +
                               "Scene ビューや Game ビューで確認できます。\n\n" +
                               "画像をインポートする方法:\n" +
                               "1. Project ビューに画像ファイルをドラッグ&ドロップ\n" +
                               "2. Inspector で Texture Type を「Sprite (2D and UI)」に設定\n" +
                               "3. Apply をクリック\n" +
                               "4. 「背景画像」や「アイコン画像」欄にドラッグ&ドロップ", MessageType.Info);
    }
    
    private void ValidateScenes()
    {
        HubPageManager manager = (HubPageManager)target;
        
        if (manager.pageButtons == null || manager.pageButtons.Count == 0)
        {
            EditorUtility.DisplayDialog("情報", "ボタンが追加されていません。", "OK");
            return;
        }
        
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== シーン検証結果 ===\n");
        
        bool allValid = true;
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var buttonData in manager.pageButtons)
        {
            if (string.IsNullOrEmpty(buttonData.sceneName))
            {
                report.AppendLine($"⚠ '{buttonData.buttonText}': シーン名が設定されていません");
                invalidCount++;
                allValid = false;
                continue;
            }
            
            // プロジェクト内のシーンを検索
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:Scene {buttonData.sceneName}");
            bool foundInProject = false;
            string scenePath = "";
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == buttonData.sceneName)
                {
                    foundInProject = true;
                    scenePath = path;
                    break;
                }
            }
            
            // ビルド設定内のシーン確認
            bool inBuildSettings = false;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string buildScenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(buildScenePath);
                if (buildSceneName == buttonData.sceneName)
                {
                    inBuildSettings = true;
                    break;
                }
            }
            
            if (!foundInProject)
            {
                report.AppendLine($"❌ '{buttonData.sceneName}': プロジェクト内に存在しません");
                invalidCount++;
                allValid = false;
            }
            else if (!inBuildSettings)
            {
                report.AppendLine($"⚠ '{buttonData.sceneName}': 見つかりましたが、ビルド設定に未追加\n   パス: {scenePath}");
                invalidCount++;
                allValid = false;
            }
            else
            {
                report.AppendLine($"✓ '{buttonData.sceneName}': OK");
                validCount++;
            }
        }
        
        report.AppendLine($"\n結果: {validCount} 個が正常、{invalidCount} 個に問題があります");
        
        if (!allValid)
        {
            report.AppendLine("\n対処方法:");
            report.AppendLine("• シーンファイルを作成: File > New Scene");
            report.AppendLine("• ビルド設定に追加: File > Build Settings");
        }
        
        Debug.Log(report.ToString());
        
        if (allValid)
        {
            EditorUtility.DisplayDialog("検証完了", "すべてのシーンが正常です！", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("検証完了", 
                $"問題が見つかりました。\n\n正常: {validCount}\n問題: {invalidCount}\n\n詳細はコンソールを確認してください。", 
                "OK");
        }
    }
    
    private void InitializeNewButton(SerializedProperty button)
    {
        button.FindPropertyRelative("sceneName").stringValue = "";
        button.FindPropertyRelative("buttonText").stringValue = "新規ボタン";
        button.FindPropertyRelative("useCustomPosition").boolValue = false;
        button.FindPropertyRelative("customPosition").vector2Value = Vector2.zero;
        button.FindPropertyRelative("anchorPreset").enumValueIndex = (int)AnchorPreset.Center;
        button.FindPropertyRelative("backgroundColor").colorValue = Color.white;
        button.FindPropertyRelative("textColor").colorValue = Color.black;
        button.FindPropertyRelative("fontSize").intValue = 24;
        button.FindPropertyRelative("buttonWidth").floatValue = 300f;
        button.FindPropertyRelative("buttonHeight").floatValue = 60f;
        button.FindPropertyRelative("enableHoverEffect").boolValue = true;
        button.FindPropertyRelative("hoverColor").colorValue = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.FindPropertyRelative("pressedColor").colorValue = new Color(0.8f, 0.8f, 0.8f, 1f);
        button.FindPropertyRelative("showBorder").boolValue = false;
        button.FindPropertyRelative("borderColor").colorValue = Color.black;
        button.FindPropertyRelative("borderWidth").floatValue = 2f;
        button.FindPropertyRelative("iconSize").floatValue = 40f;
    }
}
