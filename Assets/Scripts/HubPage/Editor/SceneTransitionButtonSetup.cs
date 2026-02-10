using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// シーン遷移ボタンシステムのセットアップヘルパー
/// </summary>
public class SceneTransitionButtonSetup : EditorWindow
{
    private string sceneName = "HubScene";
    private bool createSampleButtons = true;
    private int numberOfButtons = 3;
    private bool createSampleScenes = true;
    private bool addToBuildSettings = true;
    
    [MenuItem("Tools/シーン遷移ボタン/セットアップウィザード")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneTransitionButtonSetup>("ボタンセットアップ");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }
    
    private void OnGUI()
    {
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("シーン遷移ボタンシステム セットアップ", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("このウィザードを使用して、シーン遷移ボタンシステムを自動セットアップできます。", MessageType.Info);
        
        GUILayout.Space(20);
        
        // 基本設定
        EditorGUILayout.LabelField("基本設定", EditorStyles.boldLabel);
        sceneName = EditorGUILayout.TextField("シーン名", sceneName);
        
        GUILayout.Space(10);
        
        // サンプルボタン
        EditorGUILayout.LabelField("サンプルボタン", EditorStyles.boldLabel);
        createSampleButtons = EditorGUILayout.Toggle("サンプルボタンを作成", createSampleButtons);
        
        if (createSampleButtons)
        {
            EditorGUI.indentLevel++;
            numberOfButtons = EditorGUILayout.IntSlider("ボタン数", numberOfButtons, 1, 10);
            createSampleScenes = EditorGUILayout.Toggle("サンプルシーンも作成", createSampleScenes);
            addToBuildSettings = EditorGUILayout.Toggle("ビルド設定に追加", addToBuildSettings);
            EditorGUI.indentLevel--;
            
            if (createSampleScenes)
            {
                EditorGUILayout.HelpBox($"Scene1 〜 Scene{numberOfButtons} が Assets/Scenes フォルダに作成されます。", MessageType.Info);
            }
        }
        
        GUILayout.Space(20);
        
        // セットアップボタン
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("セットアップを実行", GUILayout.Height(40)))
        {
            SetupSceneTransitionButtons();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(20);
        
        // ユーティリティセクション
        EditorGUILayout.LabelField("ユーティリティ", EditorStyles.boldLabel);
        
        if (GUILayout.Button("画像インポートガイドを表示"))
        {
            ShowImageImportGuide();
        }
        
        if (GUILayout.Button("README を開く"))
        {
            OpenReadme();
        }
        
        GUILayout.Space(20);
        
        // クイックアクション
        EditorGUILayout.LabelField("クイックアクション", EditorStyles.boldLabel);
        
        if (GUILayout.Button("サンプル画像フォルダを作成"))
        {
            CreateImageFolder();
        }
        
        if (GUILayout.Button("サンプルシーンを作成"))
        {
            CreateSampleScenes();
        }
        
        if (GUILayout.Button("既存の HubPageManager を選択"))
        {
            SelectHubPageManager();
        }
    }
    
    private void SetupSceneTransitionButtons()
    {
        // Canvas 確認または作成
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("エラー", "Canvas が見つかりません。先にCanvas を作成してください。\n\nHierarchy → 右クリック → UI → Canvas", "OK");
            return;
        }
        
        // HubPageManager 作成
        GameObject hubManagerObj = new GameObject("HubPageManager");
        hubManagerObj.transform.SetParent(canvas.transform, false);
        
        HubPageManager hubManager = hubManagerObj.AddComponent<HubPageManager>();
        
        // Button Container 作成
        GameObject containerObj = new GameObject("ButtonContainer");
        containerObj.transform.SetParent(canvas.transform, false);
        
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = Vector2.zero;
        
        hubManager.buttonContainer = containerObj.transform;
        hubManager.layoutMode = LayoutMode.Vertical;
        hubManager.buttonSpacing = 10f;
        hubManager.startPosition = new Vector2(0, -50);
        
        // サンプルボタンの作成
        if (createSampleButtons)
        {
            hubManager.pageButtons = new System.Collections.Generic.List<PageButtonData>();
            
            for (int i = 0; i < numberOfButtons; i++)
            {
                PageButtonData buttonData = new PageButtonData
                {
                    sceneName = $"Scene{i + 1}",
                    buttonText = $"シーン {i + 1} へ移動",
                    backgroundColor = GetSampleColor(i),
                    textColor = Color.white,
                    fontSize = 24,
                    buttonWidth = 300f,
                    buttonHeight = 60f,
                    enableHoverEffect = true,
                    hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f),
                    pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f),
                    showBorder = true,
                    borderColor = Color.white,
                    borderWidth = 2f
                };
                
                hubManager.pageButtons.Add(buttonData);
            }
            
            // サンプルシーンの作成
            if (createSampleScenes)
            {
                CreateSampleScenes();
            }
            
            // ビルド設定に追加
            if (addToBuildSettings)
            {
                AddScenesToBuild();
            }
        }
        
        // ボタンを生成
        hubManager.RegenerateButtons();
        EditorUtility.SetDirty(hubManager);
        
        // 選択
        Selection.activeGameObject = hubManagerObj;
        
        EditorUtility.DisplayDialog("完了", 
            "セットアップが完了しました！\n\n" +
            "✓ HubPageManager が作成されました\n" +
            "✓ ボタンが表示されています（Scene/Game ビューで確認）\n" +
            (createSampleScenes ? "✓ サンプルシーンが作成されました\n" : "") +
            (addToBuildSettings ? "✓ シーンがビルド設定に追加されました\n" : "") +
            "\nInspector でボタンをカスタマイズできます。\n" +
            "Play ボタンで動作を確認してください！", 
            "OK");
    }
    
    private Color GetSampleColor(int index)
    {
        Color[] colors = new Color[]
        {
            new Color(0.2f, 0.6f, 1f, 1f),    // 青
            new Color(0.3f, 0.8f, 0.4f, 1f),  // 緑
            new Color(1f, 0.5f, 0.2f, 1f),    // オレンジ
            new Color(0.9f, 0.3f, 0.5f, 1f),  // ピンク
            new Color(0.7f, 0.4f, 0.9f, 1f),  // 紫
            new Color(1f, 0.8f, 0.2f, 1f),    // 黄色
            new Color(0.4f, 0.8f, 0.8f, 1f),  // シアン
            new Color(1f, 0.4f, 0.4f, 1f),    // 赤
            new Color(0.6f, 0.6f, 0.6f, 1f),  // グレー
            new Color(0.3f, 0.5f, 0.3f, 1f)   // 深緑
        };
        
        return colors[index % colors.Length];
    }
    
    private void ShowImageImportGuide()
    {
        EditorUtility.DisplayDialog("画像インポートガイド",
            "画像をボタンに使用する手順:\n\n" +
            "1. Project ビューで右クリック → Create → Folder\n" +
            "   フォルダ名: ButtonImages\n\n" +
            "2. 画像ファイル（PNG, JPG等）をこのフォルダにドラッグ&ドロップ\n\n" +
            "3. 画像を選択 → Inspector で以下を設定:\n" +
            "   • Texture Type: Sprite (2D and UI)\n" +
            "   • Apply をクリック\n\n" +
            "4. HubPageManager の Inspector で:\n" +
            "   • ボタンを展開 → 画像設定\n" +
            "   • 背景画像 または アイコン画像 にドラッグ&ドロップ\n\n" +
            "完了！",
            "OK");
    }
    
    private void OpenReadme()
    {
        string readmePath = "Assets/Scripts/HubPage/README.md";
        
        if (File.Exists(readmePath))
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(readmePath, 1);
        }
        else
        {
            EditorUtility.DisplayDialog("エラー", "README.md が見つかりません。", "OK");
        }
    }
    
    private void CreateImageFolder()
    {
        string folderPath = "Assets/Resources/ButtonImages";
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完了", 
                $"画像フォルダを作成しました:\n{folderPath}\n\n" +
                "このフォルダに画像をドラッグ&ドロップしてください。", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("情報", 
                "フォルダは既に存在します:\n" + folderPath, 
                "OK");
        }
        
        // フォルダを選択
        Object obj = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
    
    private void SelectHubPageManager()
    {
        HubPageManager manager = FindFirstObjectByType<HubPageManager>();
        
        if (manager != null)
        {
            Selection.activeGameObject = manager.gameObject;
            EditorGUIUtility.PingObject(manager.gameObject);
        }
        else
        {
            EditorUtility.DisplayDialog("情報", 
                "HubPageManager が見つかりません。\n\n" +
                "先にセットアップを実行してください。", 
                "OK");
        }
    }
    
    private void CreateSampleScenes()
    {
        string scenesFolder = "Assets/Scenes";
        
        // Scenes フォルダを作成
        if (!Directory.Exists(scenesFolder))
        {
            Directory.CreateDirectory(scenesFolder);
            AssetDatabase.Refresh();
        }
        
        List<string> createdScenes = new List<string>();
        
        for (int i = 1; i <= numberOfButtons; i++)
        {
            string sceneName = $"Scene{i}";
            string scenePath = $"{scenesFolder}/{sceneName}.unity";
            
            // シーンが既に存在する場合はスキップ
            if (File.Exists(scenePath))
            {
                Debug.Log($"シーン '{sceneName}' は既に存在します。スキップします。");
                continue;
            }
            
            // 新しいシーンを作成
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            
            // シーンに戻るボタンを追加
            AddBackButtonToScene(newScene, i);
            
            // シーンを保存
            EditorSceneManager.SaveScene(newScene, scenePath);
            EditorSceneManager.CloseScene(newScene, true);
            
            createdScenes.Add(sceneName);
            Debug.Log($"シーン '{sceneName}' を作成しました: {scenePath}");
        }
        
        AssetDatabase.Refresh();
        
        if (createdScenes.Count > 0)
        {
            EditorUtility.DisplayDialog("完了", 
                $"{createdScenes.Count} 個のシーンを作成しました:\n\n" +
                string.Join("\n", createdScenes) +
                $"\n\n場所: {scenesFolder}", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("情報", 
                "すべてのシーンは既に存在しています。", 
                "OK");
        }
    }
    
    private void AddBackButtonToScene(UnityEngine.SceneManagement.Scene scene, int sceneNumber)
    {
        // Canvas を作成
        GameObject canvasObj = new GameObject("Canvas");
        SceneManager.MoveGameObjectToScene(canvasObj, scene);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // EventSystem を作成
        GameObject eventSystemObj = new GameObject("EventSystem");
        SceneManager.MoveGameObjectToScene(eventSystemObj, scene);
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // テキストラベル作成
        GameObject textObj = new GameObject("SceneLabel");
        textObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"Scene {sceneNumber}";
        text.fontSize = 48;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(400, 100);
        textRect.anchoredPosition = new Vector2(0, 100);
        
        // 背景パネル
        GameObject panelObj = new GameObject("Background");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelObj.transform.SetAsFirstSibling();
    }
    
    private void AddScenesToBuild()
    {
        // 現在のビルド設定を取得
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        int addedCount = 0;
        
        for (int i = 1; i <= numberOfButtons; i++)
        {
            string sceneName = $"Scene{i}";
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            
            // シーンが存在するか確認
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"シーン '{scenePath}' が見つかりません。スキップします。");
                continue;
            }
            
            // 既にビルド設定に含まれているか確認
            bool alreadyInBuild = scenes.Exists(s => s.path == scenePath);
            
            if (!alreadyInBuild)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                addedCount++;
                Debug.Log($"シーン '{sceneName}' をビルド設定に追加しました。");
            }
        }
        
        // ビルド設定を更新
        EditorBuildSettings.scenes = scenes.ToArray();
        
        if (addedCount > 0)
        {
            Debug.Log($"{addedCount} 個のシーンをビルド設定に追加しました。");
            EditorUtility.DisplayDialog("完了", 
                $"{addedCount} 個のシーンをビルド設定に追加しました。\n\n" +
                "File > Build Settings で確認できます。", 
                "OK");
        }
        else
        {
            Debug.Log("すべてのシーンは既にビルド設定に含まれています。");
        }
    }
}
