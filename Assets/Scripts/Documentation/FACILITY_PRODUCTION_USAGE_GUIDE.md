# Facility自動生成機能 - 使い方ガイド（TimerManager統合版）

**最終更新**: 2026年2月12日  
**バージョン**: 2.0（TimerManager統合完了）

## 目次
1. [概要](#概要)
2. [クイックスタート](#クイックスタート)
3. [セットアップ手順](#セットアップ手順)
4. [基本的な使い方](#基本的な使い方)
5. [TimerManagerとの連携](#timerManagerとの連携)
6. [実践例](#実践例)
7. [トラブルシューティング](#トラブルシューティング)

---

## 概要

Facility自動生成機能は、ゲーム内の各施設（Facility）が一定間隔で自動的にお金やアイテムを生成する機能です。

### 主な特徴
- ✅ お金、アイテム、または両方を自動生成
- ✅ TimerManagerと完全統合（ゲーム時間ベース）
- ✅ タイマー一時停止時は生成も自動停止
- ✅ タイマー終了時に統計を自動表示
- ✅ TimerManagerがない場合のフォールバック機能
- ✅ 生成統計の自動記録

### 対応環境
- TimerManagerあり: ゲーム時間ベースで完全連携
- TimerManagerなし: Time.timeを使用したフォールバック動作

---

## クイックスタート

### 最小構成での動作確認（5分で完了）

#### ステップ1: シーンの準備
```
1. シーンにTimerManagerが存在することを確認
   （なければ自動的にフォールバックモードで動作）
   
2. 空のGameObjectを作成して「FacilityProductionManager」と命名
   
3. FacilityProductionControllerスクリプトをアタッチ
```

#### ステップ2: FacilityDataの作成
```
1. Project > 右クリック > Create > Game > Facility Data

2. Inspectorで以下を設定:
   - Facility Id: "test_facility"
   - Facility Name: "テスト施設"
   - Production Type: Money
   - Money Amount: 100
   - Production Interval: 5.0
   - Enable Auto Production: ✓
```

#### ステップ3: コードでの登録
```csharp
using UnityEngine;

public class FacilitySetupExample : MonoBehaviour
{
    public FacilityData facilityData;
    
    void Start()
    {
        // Facilityを作成（レベル1、解放済み）
        Facility facility = facilityData.CreateFacility(1, true);
        
        // 生成コントローラーに登録
        FacilityProductionController controller = 
            FindObjectOfType<FacilityProductionController>();
        controller.RegisterFacility(facility);
        
        // TimerManagerを開始（存在する場合）
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.StartTimer();
        }
    }
}
```

#### ステップ4: 実行して確認
```
Play ボタンを押すと、5秒ごとにコンソールに以下が表示されます：
"[テスト施設] お金を生成: 100"
```

---

## セットアップ手順

### 1. 必要なコンポーネント

#### 必須
- **FacilityProductionController**: 生成管理コントローラー
- **FacilityData**: 施設のマスターデータ（ScriptableObject）

#### 推奨
- **TimerManager**: ゲーム時間管理（自動連携）
- **GameDatabase**: お金・アイテム管理（TODO実装待ち）

### 2. シーンへの配置

```
Hierarchy:
  ├── TimerManager (DontDestroyOnLoad)
  ├── FacilityProductionManager
  │     └── FacilityProductionController
  └── （その他のゲームオブジェクト）
```

### 3. FacilityProductionControllerの設定

| プロパティ | 説明 | 推奨値 |
|-----------|------|--------|
| Show Debug Log | デバッグログを表示 | ✓（開発中） |

---

## 基本的な使い方

### FacilityDataの設定パターン

#### パターン1: お金のみ生成
```
【Inspector設定】
Production Type: Money
Money Amount: 500
Production Interval: 10.0
Enable Auto Production: ✓
```
**結果**: 10秒ごとに500のお金を生成

---

#### パターン2: アイテムのみ生成
```
【Inspector設定】
Production Type: Item
Item Productions: [配列サイズ 1]
  Element 0:
    Item Id: "wood"
    Quantity: 10
Production Interval: 15.0
Enable Auto Production: ✓
```
**結果**: 15秒ごとに木材10個を生成

---

#### パターン3: お金とアイテム両方
```
【Inspector設定】
Production Type: Both
Money Amount: 200
Item Productions: [配列サイズ 2]
  Element 0:
    Item Id: "wood"
    Quantity: 5
  Element 1:
    Item Id: "stone"
    Quantity: 3
Production Interval: 20.0
Enable Auto Production: ✓
```
**結果**: 20秒ごとにお金200、木材5個、石3個を生成

---

### コードでの動的設定

#### 施設の登録
```csharp
public class FacilityManager : MonoBehaviour
{
    public FacilityData mineData;
    private FacilityProductionController productionController;
    
    void Start()
    {
        productionController = FindObjectOfType<FacilityProductionController>();
        
        // 施設を作成して登録
        Facility mine = mineData.CreateFacility(1, true);
        productionController.RegisterFacility(mine);
    }
}
```

#### 生成設定の変更
```csharp
// お金の生成量を変更
facility.SetMoneyAmount(1000);

// 生成間隔を変更（30秒に）
facility.SetProductionInterval(30f);

// アイテムを追加
facility.AddItemProduction("gold_ore", 5);

// 生成タイプを変更
facility.SetProductionType(ProductionType.Both);
```

#### 一時停止・再開
```csharp
// 特定の施設を一時停止
productionController.SetFacilityProductionPaused("facility_id", true);

// 全施設を一時停止
productionController.SetAllProductionPaused(true);

// 再開
productionController.SetAllProductionPaused(false);
```

#### 手動生成の実行
```csharp
// 間隔を待たずに即座に生成
productionController.ManualProduceResources("facility_id");
```

---

## TimerManagerとの連携

### 自動連携機能

TimerManagerが存在する場合、以下が自動的に行われます：

#### 1. タイマー開始時
```csharp
TimerManager.Instance.StartTimer();
```
- 全Facilityの`lastProductionTime`がリセットされる
- 一時停止が解除され、生成が開始される
- コンソールに「タイマーが開始されました。生成を開始します。」と表示

#### 2. タイマー一時停止時
```csharp
TimerManager.Instance.TogglePause();
```
- 全Facilityの生成が自動的に停止
- 再開すると生成も再開
- コンソールに「タイマーが一時停止されました。」と表示

#### 3. タイマー終了時
```csharp
// タイムアウト時、自動的に呼ばれる
```
- 全Facilityの生成が停止
- 生成統計が自動的にコンソールに表示
- 各Facilityの累計生成回数、総お金、総アイテムが確認可能

### 時間管理の仕組み

```csharp
// TimerManagerがある場合
float currentTime = TimerManager.Instance.CurrentTime;

// TimerManagerがない場合（フォールバック）
float currentTime = Time.time;
```

生成間隔のチェックにTimerManagerの`CheckInterval()`メソッドを使用：
```csharp
if (TimerManager.Instance.CheckInterval(
    facility.productionConfig.productionInterval,
    ref facility.productionState.lastProductionTime))
{
    // 生成実行
}
```

### フォールバックモード

TimerManagerが存在しない場合：
- 自動的にTime.timeを使用
- 警告ログ「TimerManagerが見つかりません...」が表示
- 基本的な生成機能は正常に動作
- タイマーイベントは受け取らない

---

## 実践例

### 例1: 木材生産小屋

```csharp
public class LumbermillSetup : MonoBehaviour
{
    public FacilityData lumbermillData;
    
    void Start()
    {
        // 施設作成
        Facility lumbermill = lumbermillData.CreateFacility(1, true);
        
        // 生成設定
        lumbermill.SetProductionType(ProductionType.Item);
        lumbermill.AddItemProduction("wood", 10);
        lumbermill.SetProductionInterval(30f); // 30秒ごと
        
        // 登録
        var controller = FindObjectOfType<FacilityProductionController>();
        controller.RegisterFacility(lumbermill);
    }
}
```

---

### 例2: 金鉱山（お金とアイテム両方）

```csharp
public class GoldMineSetup : MonoBehaviour
{
    public FacilityData goldMineData;
    
    void Start()
    {
        Facility goldMine = goldMineData.CreateFacility(1, true);
        
        // お金とアイテム両方を生成
        goldMine.SetProductionType(ProductionType.Both);
        goldMine.SetMoneyAmount(500);
        goldMine.AddItemProduction("gold_ore", 5);
        goldMine.AddItemProduction("silver_ore", 3);
        goldMine.SetProductionInterval(60f); // 1分ごと
        
        var controller = FindObjectOfType<FacilityProductionController>();
        controller.RegisterFacility(goldMine);
    }
}
```

---

### 例3: レベルアップ時の生成量増加

```csharp
public class FacilityUpgrade : MonoBehaviour
{
    public void UpgradeFacility(Facility facility)
    {
        // レベルアップ
        facility.level++;
        
        // レベルに応じて生成量を増加
        if (facility.productionConfig.ProducesMoney())
        {
            int baseAmount = 100;
            int newAmount = baseAmount * facility.level;
            facility.SetMoneyAmount(newAmount);
        }
        
        // アイテムの生成量も増加
        foreach (var itemProd in facility.productionConfig.itemProductions)
        {
            itemProd.quantity = 5 * facility.level;
        }
        
        Debug.Log($"{facility.facilityName} をレベル{facility.level}にアップグレードしました！");
    }
}
```

---

### 例4: 条件付き生成制御

```csharp
public class ConditionalProduction : MonoBehaviour
{
    private Facility workshop;
    private FacilityProductionController controller;
    
    void Start()
    {
        controller = FindObjectOfType<FacilityProductionController>();
        // workshop の初期化...
    }
    
    void Update()
    {
        // 昼間のみ生成（例）
        bool isDaytime = TimerManager.Instance.CurrentTime % 24 < 12;
        
        if (isDaytime && !workshop.productionState.isPaused)
        {
            workshop.SetAutoProductionEnabled(true);
        }
        else
        {
            workshop.SetAutoProductionEnabled(false);
        }
    }
}
```

---

### 例5: 統計の取得と表示

```csharp
public class ProductionStatsUI : MonoBehaviour
{
    public FacilityProductionController controller;
    public string facilityId;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowStats();
        }
    }
    
    void ShowStats()
    {
        ProductionState state = controller.GetFacilityProductionState(facilityId);
        
        if (state != null)
        {
            Debug.Log("=== 生成統計 ===");
            Debug.Log($"生成回数: {state.totalProductionCount}");
            Debug.Log($"総お金: {state.totalMoneyProduced}");
            
            foreach (var item in state.totalItemsProduced)
            {
                Debug.Log($"{item.Key}: {item.Value}個");
            }
        }
    }
}
```

---

## トラブルシューティング

### 生成が開始されない

#### 確認項目
1. **Facilityが解放されているか**
   ```csharp
   Debug.Log($"isUnlocked: {facility.isUnlocked}"); // trueである必要がある
   ```

2. **自動生成が有効か**
   ```csharp
   Debug.Log($"enableAutoProduction: {facility.productionConfig.enableAutoProduction}");
   ```

3. **生成タイプが設定されているか**
   ```csharp
   Debug.Log($"productionType: {facility.productionConfig.productionType}");
   // Noneになっていないこと
   ```

4. **FacilityProductionControllerに登録されているか**
   ```csharp
   controller.RegisterFacility(facility);
   ```

5. **TimerManagerが開始されているか**
   ```csharp
   if (TimerManager.Instance != null)
   {
       TimerManager.Instance.StartTimer();
   }
   ```

---

### 生成間隔がおかしい

#### 原因と対処

**原因1**: TimerManagerとTime.timeが混在
```csharp
// ❌ 間違い
facility.productionState.lastProductionTime = Time.time;

// ✅ 正しい（TimerManagerがある場合）
facility.productionState.lastProductionTime = TimerManager.Instance.CurrentTime;
```

**原因2**: 一時停止状態
```csharp
// 確認
Debug.Log($"isPaused: {facility.productionState.isPaused}");

// 解除
controller.SetFacilityProductionPaused(facilityId, false);
```

---

### デバッグログが表示されない

FacilityProductionControllerのInspectorで：
```
Show Debug Log: ✓
```
にチェックを入れてください。

---

### TimerManagerが見つからない警告

```
警告: FacilityProductionController: TimerManagerが見つかりません。
```

**対処方法**:
1. シーンにTimerManagerを配置
2. またはフォールバックモードで使用（Time.time使用）

フォールバックモードでも基本機能は動作します。

---

### GameDatabaseに反映されない

現在、GameDatabaseとの連携は実装待ちです。

該当箇所（FacilityProductionController.cs）:
```csharp
// TODO: GameDatabaseにお金を追加
// GameDatabase.Instance.AddMoney(moneyProduced);

// TODO: GameDatabaseにアイテムを追加
// GameDatabase.Instance.AddItem(itemId, quantity);
```

生成は正常に行われており、統計にも記録されています。
GameDatabaseへの反映は今後のアップデートで対応予定です。

---

## データ構造リファレンス

### ProductionType（列挙型）
| 値 | 説明 |
|----|------|
| None | 生成なし |
| Money | お金のみ生成 |
| Item | アイテムのみ生成 |
| Both | お金とアイテム両方生成 |

### ProductionConfig（生成設定）
| フィールド | 型 | 説明 |
|-----------|-----|------|
| productionType | ProductionType | 生成タイプ |
| moneyAmount | int | 生成する金額 |
| itemProductions | List\<ItemProduction\> | 生成アイテムリスト |
| productionInterval | float | 生成間隔（秒） |
| enableAutoProduction | bool | 自動生成の有効/無効 |

### ItemProduction（アイテム生成設定）
| フィールド | 型 | 説明 |
|-----------|-----|------|
| itemId | string | アイテムID |
| itemData | ItemData | ItemDataへの参照（推奨） |
| quantity | int | 生成数量 |

### ProductionState（生成状態）
| フィールド | 型 | 説明 |
|-----------|-----|------|
| lastProductionTime | float | 最後に生成した時刻 |
| totalProductionCount | int | 累計生成回数 |
| totalMoneyProduced | long | 累計お金生成量 |
| totalItemsProduced | Dictionary\<string,int\> | 累計アイテム生成量 |
| isPaused | bool | 一時停止中 |

---

## よくある質問（FAQ）

### Q1: 複数の施設を一度に登録できますか？
```csharp
List<FacilityData> facilityDataList = new List<FacilityData>();

foreach (var data in facilityDataList)
{
    Facility facility = data.CreateFacility(1, true);
    controller.RegisterFacility(facility);
}
```

### Q2: 生成間隔を動的に変更できますか？
はい、いつでも変更可能です：
```csharp
facility.SetProductionInterval(newInterval);
```

### Q3: 特定のアイテムだけ生成量を変更できますか？
```csharp
foreach (var itemProd in facility.productionConfig.itemProductions)
{
    if (itemProd.itemId == "wood")
    {
        itemProd.quantity = 20; // 木材だけ20個に
    }
}
```

### Q4: TimerManagerなしでも使えますか？
はい、フォールバックモードで動作します（Time.time使用）。

### Q5: シーンをまたいで生成は継続されますか？
FacilityProductionControllerをDontDestroyOnLoadに設定すれば可能です：
```csharp
DontDestroyOnLoad(gameObject);
```

---

## 関連ドキュメント

- **TIMER_INTEGRATION_GUIDE.md**: TimerManager統合の技術詳細
- **README_FacilityProduction.md**: 開発者向け技術仕様書

---

## 更新履歴

| 日付 | バージョン | 内容 |
|------|-----------|------|
| 2026/02/12 | 2.0 | TimerManager統合完了版にアップデート |
| 2026/02/XX | 1.0 | 初版作成 |

---

**このドキュメントについて**  
ご不明な点やバグ報告は、プロジェクト管理者までお問い合わせください。
