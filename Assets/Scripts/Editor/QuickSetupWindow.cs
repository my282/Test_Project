using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// クイックセットアップウィンドウ
/// </summary>
public class QuickSetupWindow : EditorWindow
{
    private int itemCount = 5;
    private int facilityCount = 3;

    [MenuItem("Game/Quick Setup Database")]
    public static void ShowWindow()
    {
        GetWindow<QuickSetupWindow>("Database Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("マスターデータベース クイックセットアップ", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox("テスト用のアイテムと設備を自動生成します", MessageType.Info);
        EditorGUILayout.Space(5);

        itemCount = EditorGUILayout.IntField("アイテム数:", itemCount);
        facilityCount = EditorGUILayout.IntField("設備数:", facilityCount);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("データベースをセットアップ", GUILayout.Height(40)))
        {
            SetupDatabase();
        }
    }

    private void SetupDatabase()
    {
        // Resourcesフォルダを作成
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // GameDataフォルダを作成
        string gameDataPath = "Assets/GameData";
        if (!AssetDatabase.IsValidFolder(gameDataPath))
        {
            AssetDatabase.CreateFolder("Assets", "GameData");
        }

        string itemsPath = gameDataPath + "/Items";
        if (!AssetDatabase.IsValidFolder(itemsPath))
        {
            AssetDatabase.CreateFolder("Assets/GameData", "Items");
        }

        string facilitiesPath = gameDataPath + "/Facilities";
        if (!AssetDatabase.IsValidFolder(facilitiesPath))
        {
            AssetDatabase.CreateFolder("Assets/GameData", "Facilities");
        }

        // MasterDatabaseを作成
        string dbPath = resourcesPath + "/MasterDatabase.asset";
        MasterDatabase masterDB = AssetDatabase.LoadAssetAtPath<MasterDatabase>(dbPath);
        if (masterDB == null)
        {
            masterDB = CreateInstance<MasterDatabase>();
            AssetDatabase.CreateAsset(masterDB, dbPath);
        }

        // アイテムを作成
        ItemType[] itemTypes = { ItemType.Consumable, ItemType.Equipment, ItemType.Material, ItemType.KeyItem, ItemType.Other };
        for (int i = 1; i <= itemCount; i++)
        {
            string itemPath = $"{itemsPath}/item{i}.asset";
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(itemPath);
            if (item == null)
            {
                item = CreateInstance<ItemData>();
                item.itemId = $"item{i}";
                item.itemName = $"アイテム{i}";
                item.description = $"これはアイテム{i}の説明です";
                item.type = itemTypes[i % itemTypes.Length];
                item.maxStackSize = 99;
                item.basePrice = i * 100;
                AssetDatabase.CreateAsset(item, itemPath);
                
                masterDB.AddItemData(item);
            }
        }

        // 設備を作成
        FacilityType[] facilityTypes = { FacilityType.Production, FacilityType.Storage, FacilityType.Defense, FacilityType.Research, FacilityType.Other };
        for (int i = 1; i <= facilityCount; i++)
        {
            string facilityPath = $"{facilitiesPath}/facility{i}.asset";
            FacilityData facility = AssetDatabase.LoadAssetAtPath<FacilityData>(facilityPath);
            if (facility == null)
            {
                facility = CreateInstance<FacilityData>();
                facility.facilityId = $"facility{i}";
                facility.facilityName = $"設備{i}";
                facility.description = $"これは設備{i}の説明です";
                facility.type = facilityTypes[i % facilityTypes.Length];
                facility.maxLevel = 10;
                facility.unlockMoneyCost = i * 500;
                facility.upgradeMoneyCost = i * 200;
                
                // サンプルとしてアイテムコストも追加（facility1の場合）
                if (i == 1)
                {
                    facility.unlockItemCosts.Add(new ItemCost("item1", 1));
                    facility.unlockItemCosts.Add(new ItemCost("item2", 2));
                    facility.upgradeItemCosts.Add(new ItemCost("item1", 1));
                }
                AssetDatabase.CreateAsset(facility, facilityPath);
                
                masterDB.AddFacilityData(facility);
            }
        }

        EditorUtility.SetDirty(masterDB);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"セットアップ完了！アイテム{itemCount}個、設備{facilityCount}個を作成しました。");
        EditorUtility.DisplayDialog("完了", 
            $"データベースのセットアップが完了しました！\n\n" +
            $"アイテム: {itemCount}個\n" +
            $"設備: {facilityCount}個\n\n" +
            $"Resources/MasterDatabase を確認してください。", "OK");

        // MasterDatabaseを選択
        Selection.activeObject = masterDB;
        EditorGUIUtility.PingObject(masterDB);
    }
}
