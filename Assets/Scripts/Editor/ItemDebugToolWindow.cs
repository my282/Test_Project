using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// デバッグ用アイテム取得ツール
/// Unity Editorのウィンドウからアイテムを自由に入手できます
/// </summary>
public class ItemDebugToolWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string searchText = "";
    private int selectedItemIndex = 0;
    private int itemQuantity = 1;
    private List<ItemData> filteredItems = new List<ItemData>();
    private string[] itemNames = new string[0];
    private ItemType filterType = ItemType.Consumable;
    private bool useTypeFilter = false;

    [MenuItem("Game/Debug Tools/Item Debug Tool")]
    public static void ShowWindow()
    {
        ItemDebugToolWindow window = GetWindow<ItemDebugToolWindow>("アイテムデバッグツール");
        window.minSize = new Vector2(400, 500);
    }

    private void OnEnable()
    {
        RefreshItemList();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        
        // タイトル
        EditorGUILayout.LabelField("アイテムデバッグツール", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("好きなアイテムを個数指定して入手できます", MessageType.Info);
        
        GUILayout.Space(10);

        // データベース確認
        if (MasterDatabase.Instance == null)
        {
            EditorGUILayout.HelpBox("MasterDatabaseが見つかりません！\nResources/MasterDatabase.assetが必要です。", MessageType.Error);
            if (GUILayout.Button("クイックセットアップを開く"))
            {
                QuickSetupWindow.ShowWindow();
            }
            return;
        }

        if (filteredItems.Count == 0)
        {
            EditorGUILayout.HelpBox("アイテムが登録されていません。", MessageType.Warning);
            if (GUILayout.Button("リストを更新"))
            {
                RefreshItemList();
            }
            return;
        }

        // GameDatabase確認
        bool gameDatabaseExists = GameDatabase.Instance != null;
        if (!Application.isPlaying && !gameDatabaseExists)
        {
            EditorGUILayout.HelpBox("GameDatabaseがシーンに存在しません。\nプレイモードで実行するか、シーンにGameDatabaseを配置してください。", MessageType.Warning);
            GUILayout.Space(5);
        }

        // スクロールビュー開始
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 検索・フィルター
        DrawSearchAndFilter();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);

        // アイテム選択
        DrawItemSelection();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);

        // 数量入力
        DrawQuantityInput();

        GUILayout.Space(15);

        // 実行ボタン
        DrawExecuteButton(gameDatabaseExists);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);

        // 現在の所持アイテム表示
        if (Application.isPlaying && gameDatabaseExists)
        {
            DrawCurrentInventory();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 検索とフィルター部分の描画
    /// </summary>
    private void DrawSearchAndFilter()
    {
        EditorGUILayout.LabelField("検索・フィルター", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("検索:", GUILayout.Width(50));
        string newSearchText = EditorGUILayout.TextField(searchText);
        if (newSearchText != searchText)
        {
            searchText = newSearchText;
            RefreshItemList();
        }
        if (GUILayout.Button("クリア", GUILayout.Width(60)))
        {
            searchText = "";
            RefreshItemList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        bool newUseTypeFilter = EditorGUILayout.Toggle("タイプフィルター:", useTypeFilter, GUILayout.Width(150));
        GUI.enabled = useTypeFilter;
        ItemType newFilterType = (ItemType)EditorGUILayout.EnumPopup(filterType);
        GUI.enabled = true;

        if (newUseTypeFilter != useTypeFilter || newFilterType != filterType)
        {
            useTypeFilter = newUseTypeFilter;
            filterType = newFilterType;
            RefreshItemList();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("リストを更新", GUILayout.Height(25)))
        {
            RefreshItemList();
        }
    }

    /// <summary>
    /// アイテム選択部分の描画
    /// </summary>
    private void DrawItemSelection()
    {
        EditorGUILayout.LabelField("アイテム選択", EditorStyles.boldLabel);
        
        if (filteredItems.Count > 0)
        {
            selectedItemIndex = Mathf.Clamp(selectedItemIndex, 0, filteredItems.Count - 1);
            
            int newIndex = EditorGUILayout.Popup("アイテム:", selectedItemIndex, itemNames);
            if (newIndex != selectedItemIndex)
            {
                selectedItemIndex = newIndex;
            }

            // 選択中のアイテム情報表示
            ItemData selectedItem = filteredItems[selectedItemIndex];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("選択中のアイテム情報", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("ID:", selectedItem.itemId);
            EditorGUILayout.LabelField("名前:", selectedItem.itemName);
            EditorGUILayout.LabelField("タイプ:", selectedItem.type.ToString());
            EditorGUILayout.LabelField("説明:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField(selectedItem.description, EditorStyles.wordWrappedLabel);
            if (selectedItem.icon != null)
            {
                GUILayout.Label(selectedItem.icon.texture, GUILayout.Width(64), GUILayout.Height(64));
            }
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 数量入力部分の描画
    /// </summary>
    private void DrawQuantityInput()
    {
        EditorGUILayout.LabelField("数量", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("取得個数:", GUILayout.Width(70));
        itemQuantity = EditorGUILayout.IntField(itemQuantity);
        itemQuantity = Mathf.Max(1, itemQuantity);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("1", GUILayout.Height(25))) itemQuantity = 1;
        if (GUILayout.Button("10", GUILayout.Height(25))) itemQuantity = 10;
        if (GUILayout.Button("99", GUILayout.Height(25))) itemQuantity = 99;
        if (GUILayout.Button("999", GUILayout.Height(25))) itemQuantity = 999;
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 実行ボタン部分の描画
    /// </summary>
    private void DrawExecuteButton(bool gameDatabaseExists)
    {
        GUI.enabled = Application.isPlaying && gameDatabaseExists;
        
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        
        if (GUILayout.Button($"アイテムを {itemQuantity} 個取得する", GUILayout.Height(40)))
        {
            AddItemToInventory();
        }
        
        GUI.backgroundColor = originalColor;
        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("プレイモード中のみアイテムを追加できます", MessageType.Info);
        }
        else if (!gameDatabaseExists)
        {
            EditorGUILayout.HelpBox("GameDatabaseが見つかりません", MessageType.Warning);
        }
    }

    /// <summary>
    /// 現在のインベントリ表示
    /// </summary>
    private void DrawCurrentInventory()
    {
        EditorGUILayout.LabelField("現在の所持アイテム", EditorStyles.boldLabel);
        
        List<Item> currentItems = GameDatabase.Instance.GetAllItems();
        
        if (currentItems.Count == 0)
        {
            EditorGUILayout.HelpBox("アイテムを所持していません", MessageType.Info);
        }
        else
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (Item item in currentItems)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{item.itemName} x {item.quantity}", GUILayout.Width(200));
                EditorGUILayout.LabelField($"[{item.type}]", GUILayout.Width(100));
                if (GUILayout.Button("削除", GUILayout.Width(50)))
                {
                    GameDatabase.Instance.RemoveItem(item.itemId, item.quantity);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            if (GUILayout.Button("全アイテムをクリア", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("確認", 
                    "すべてのアイテムを削除しますか？", 
                    "削除", "キャンセル"))
                {
                    ClearAllItems();
                }
            }
        }
    }

    /// <summary>
    /// アイテムリストを更新
    /// </summary>
    private void RefreshItemList()
    {
        if (MasterDatabase.Instance == null)
        {
            filteredItems.Clear();
            itemNames = new string[0];
            return;
        }

        List<ItemData> allItems = MasterDatabase.Instance.GetAllItemData();
        
        // フィルター適用
        filteredItems = allItems.Where(item =>
        {
            bool matchSearch = string.IsNullOrEmpty(searchText) ||
                               item.itemName.Contains(searchText) ||
                               item.itemId.Contains(searchText) ||
                               item.description.Contains(searchText);
            
            bool matchType = !useTypeFilter || item.type == filterType;
            
            return matchSearch && matchType;
        }).ToList();

        // 表示用の名前配列を作成
        itemNames = filteredItems.Select(item => $"{item.itemName} ({item.itemId})").ToArray();
        
        selectedItemIndex = Mathf.Clamp(selectedItemIndex, 0, Mathf.Max(0, filteredItems.Count - 1));
    }

    /// <summary>
    /// アイテムをインベントリに追加
    /// </summary>
    private void AddItemToInventory()
    {
        if (filteredItems.Count == 0 || selectedItemIndex >= filteredItems.Count)
        {
            Debug.LogWarning("有効なアイテムが選択されていません");
            return;
        }

        ItemData selectedItem = filteredItems[selectedItemIndex];
        
        GameDatabase.Instance.AddItem(
            selectedItem.itemId,
            selectedItem.itemName,
            selectedItem.description,
            itemQuantity,
            selectedItem.type,
            selectedItem.icon
        );

        Debug.Log($"[デバッグツール] {selectedItem.itemName} を {itemQuantity} 個追加しました");
    }

    /// <summary>
    /// すべてのアイテムをクリア
    /// </summary>
    private void ClearAllItems()
    {
        List<Item> allItems = GameDatabase.Instance.GetAllItems();
        foreach (Item item in allItems.ToList())
        {
            GameDatabase.Instance.RemoveItem(item.itemId, item.quantity);
        }
        Debug.Log("[デバッグツール] すべてのアイテムをクリアしました");
    }
}
