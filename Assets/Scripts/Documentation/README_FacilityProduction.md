# Facility自動生成機能 - 技術仕様書

**最終更新**: 2026年2月12日  
**ステータス**: ✅ TimerManager統合完了

> **注意**: このドキュメントは開発者向けの技術仕様書です。  
> 使い方ガイドは **FACILITY_PRODUCTION_USAGE_GUIDE.md** をご覧ください。

## 概要
各Facilityに自動生成機能を追加しました。TimerManagerと完全統合し、ゲーム時間ベースでお金やアイテムを自動生成できます。

## 作成したファイル

```
Assets/Scripts/
├── FacilityProduction.cs              # 生成設定のデータ構造
├── Facility.cs                        # 拡張済み（生成機能追加）
├── FacilityData.cs                    # 拡張済み（生成設定追加）
├── FacilityProductionController.cs   # 生成処理の管理
└── TIMER_INTEGRATION_GUIDE.md         # タイマー統合手順書
```

## 基本的な使い方

### 1. FacilityDataの設定（Inspector）

1. FacilityDataのScriptableObjectを開く
2. 「自動生成設定」セクションで以下を設定：

#### お金のみ生成する場合
```
Production Type: Money
Money Amount: 100
Production Interval: 10.0
Enable Auto Production: ✓
```

#### アイテムのみ生成する場合
```
Production Type: Item
Item Productions:
  - Item Id: "wood"
    Quantity: 5
Production Interval: 15.0
Enable Auto Production: ✓
```

#### お金とアイテム両方を生成する場合
```
Production Type: Both
Money Amount: 50
Item Productions:
  - Item Id: "wood"
    Quantity: 3
  - Item Id: "stone"
    Quantity: 2
Production Interval: 20.0
Enable Auto Production: ✓
```

### 2. FacilityProductionControllerのセットアップ

1. シーンに空のGameObjectを作成（例: "FacilityProductionManager"）
2. FacilityProductionControllerスクリプトをアタッチ
3. デバッグログを表示したい場合は"Show Debug Log"にチェック

### 3. Facilityの登録

```csharp
// Facilityを作成
FacilityData facilityData = /* ScriptableObjectの参照 */;
Facility facility = facilityData.CreateFacility(level: 1, isUnlocked: true);

// 生成コントローラーに登録
FacilityProductionController controller = FindObjectOfType<FacilityProductionController>();
controller.RegisterFacility(facility);
```

## コードでの設定変更

### 生成タイプの変更

```csharp
// お金のみ生成に変更
facility.SetProductionType(ProductionType.Money);
facility.SetMoneyAmount(200);

// アイテムのみ生成に変更
facility.SetProductionType(ProductionType.Item);
facility.AddItemProduction("gold_ore", 10);

// 両方生成に変更
facility.SetProductionType(ProductionType.Both);
facility.SetMoneyAmount(100);
facility.AddItemProduction("iron_ore", 5);
```

### 生成間隔の変更

```csharp
// 5秒ごとに生成
facility.SetProductionInterval(5f);

// 1分ごとに生成
facility.SetProductionInterval(60f);
```

### 自動生成のON/OFF

```csharp
// 自動生成を停止
facility.SetAutoProductionEnabled(false);

// 自動生成を再開
facility.SetAutoProductionEnabled(true);
```

### アイテム生成の追加

```csharp
// アイテムIDで追加
facility.AddItemProduction("diamond", 1);

// ItemDataで追加（推奨）
ItemData itemData = /* ItemDataの参照 */;
facility.AddItemProduction(itemData, 3);
```

## FacilityProductionControllerの操作

### 手動で生成を実行

```csharp
// 特定のFacilityを手動で生成実行
controller.ManualProduceResources("facility_mine_01");
```

### 一時停止/再開

```csharp
// 特定のFacilityを一時停止
controller.SetFacilityProductionPaused("facility_mine_01", true);

// すべてのFacilityを一時停止
controller.SetAllProductionPaused(true);

// 再開
controller.SetAllProductionPaused(false);
```

### 統計情報の取得

```csharp
// 特定のFacilityの統計を取得
ProductionState state = controller.GetFacilityProductionState("facility_mine_01");
Debug.Log($"生成回数: {state.totalProductionCount}");
Debug.Log($"総お金: {state.totalMoneyProduced}");

// すべての統計をログ出力
controller.ShowAllProductionStats();
```

### 統計のリセット

```csharp
// 特定のFacilityをリセット
facility.ResetProductionState();

// すべてをリセット
controller.ResetAllProductionStates();
```

## 実装例

### 例1: 木材生産施設

```csharp
// FacilityDataの設定（Inspector）
// Production Type: Item
// Item Productions:
//   - Item Id: "wood"
//     Quantity: 10
// Production Interval: 30.0

// コードでの設定
Facility sawmill = sawmillData.CreateFacility(1, true);
sawmill.SetProductionType(ProductionType.Item);
sawmill.AddItemProduction("wood", 10);
sawmill.SetProductionInterval(30f);

controller.RegisterFacility(sawmill);
```

### 例2: 金鉱山（お金とアイテム両方）

```csharp
Facility goldMine = goldMineData.CreateFacility(1, true);
goldMine.SetProductionType(ProductionType.Both);
goldMine.SetMoneyAmount(500);
goldMine.AddItemProduction("gold_ore", 5);
goldMine.SetProductionInterval(60f);

controller.RegisterFacility(goldMine);
```

### 例3: レベルアップで生成量増加

```csharp
void UpgradeFacility(Facility facility)
{
    facility.level++;
    
    // レベルに応じて生成量を増加
    if (facility.productionConfig.ProducesMoney())
    {
        int newAmount = facility.productionConfig.moneyAmount + (facility.level * 50);
        facility.SetMoneyAmount(newAmount);
    }
    
    // アイテムの生成量も増加
    foreach (var itemProd in facility.productionConfig.itemProductions)
    {
        itemProd.quantity += facility.level;
    }
}
```

### 例4: 条件付き生成

```csharp
// カスタムコントローラーの作成
public class CustomFacilityController : MonoBehaviour
{
    private FacilityProductionController productionController;
    private Facility workshop;
    
    void Start()
    {
        productionController = GetComponent<FacilityProductionController>();
        workshop = /* Facilityの参照 */;
        productionController.RegisterFacility(workshop);
    }
    
    void Update()
    {
        // プレイヤーが近くにいる時だけ生成
        if (IsPlayerNearby())
        {
            workshop.SetAutoProductionEnabled(true);
        }
        else
        {
            workshop.SetAutoProductionEnabled(false);
        }
    }
    
    bool IsPlayerNearby()
    {
        // プレイヤーとの距離チェック
        return true; // 実装例
    }
}
```

## データ構造の説明

### ProductionType（列挙型）
- `None`: 生成なし
- `Money`: お金のみ生成
- `Item`: アイテムのみ生成
- `Both`: お金とアイテム両方生成

### ProductionConfig（生成設定）
- `productionType`: 生成タイプ
- `moneyAmount`: 生成する金額
- `itemProductions`: 生成するアイテムのリスト
- `productionInterval`: 生成間隔（秒）
- `enableAutoProduction`: 自動生成を有効化

### ItemProduction（アイテム生成設定）
- `itemId`: アイテムID（文字列）
- `itemData`: ItemDataへの参照（推奨）
- `quantity`: 生成数量

### ProductionState（生成状態）
- `lastProductionTime`: 最後に生成した時刻
- `totalProductionCount`: 累計生成回数
- `totalMoneyProduced`: 累計お金生成量
- `totalItemsProduced`: 累計アイテム生成量（辞書）
- `isPaused`: 一時停止中かどうか

## TimerManagerとの統合

✅ **統合完了**: 2026年2月12日

### 統合内容
- ✅ `Time.time` → `TimerManager.Instance.CurrentTime`（統合完了）
- ✅ タイマー一時停止時に自動で生成停止
- ✅ タイマー終了時に統計表示
- ✅ ゲーム時間に基づく正確な間隔管理
- ✅ TimerManagerがない場合のフォールバック機能

### 動作仕様
- **TimerManagerあり**: ゲーム時間（TimerManager.CurrentTime）ベースで動作
- **TimerManagerなし**: Time.timeをフォールバックとして使用
- タイマー一時停止時は自動的に全施設の生成が停止
- タイマー終了時に全施設の統計が自動表示

詳細は **TIMER_INTEGRATION_GUIDE.md** を参照してください。

## デバッグとテスト

### デバッグログの確認

FacilityProductionControllerの"Show Debug Log"をチェックすると、以下のログが出力されます：

```
Facility 'Gold Mine' registered for production.
[Gold Mine] お金を生成: 500
[Gold Mine] アイテムを生成: gold_ore x5
[Gold Mine] 生成完了 (累計: 1回)
```

### Inspector での確認

Facilityの状態をInspectorで確認できます：
- Production Config: 現在の生成設定
- Production State: 生成統計

### 統計情報の表示

```csharp
// 右クリックメニューから実行可能
[ContextMenu("Show All Production Stats")]
```

または：

```csharp
controller.ShowAllProductionStats();
```

## トラブルシューティング

### 生成されない場合

1. Facilityが解放されているか確認（`isUnlocked = true`）
2. 自動生成が有効か確認（`enableAutoProduction = true`）
3. 生成タイプが`None`になっていないか確認
4. FacilityProductionControllerに登録されているか確認
5. 生成間隔が経過しているか確認

### アイテムが追加されない場合

現在、GameDatabaseとの連携部分は`TODO`コメントになっています。
タイマー統合時に併せて実装予定です。

該当箇所（FacilityProductionController.cs）:
```csharp
// TODO: GameDatabaseにお金を追加
// GameDatabase.Instance.AddMoney(moneyProduced);

// TODO: GameDatabaseにアイテムを追加
// GameDatabase.Instance.AddItem(itemId, quantity);
```
x] ~~TimerManagerとの統合~~ ✅ 完了（2026/02/12）
- [ ] GameDatabaseとの連携（TODO実装待ち）
- [ ] レベル別生成量の自動計算
- [ ] 条件付き生成（天候、時間帯など）
- [ ] ブースト機能（一定時間生成量アップ）
- [ ] UI表示（生成までの残り時間など）

## 関連ドキュメント

- **FACILITY_PRODUCTION_USAGE_GUIDE.md**: 使い方ガイド（ユーザー向け）
- **TIMER_INTEGRATION_GUIDE.md**: TimerManager統合完了報告書
## 関連ドキュメント

- **TIMER_INTEGRATION_GUIDE.md**: タイマー統合の詳細手順
- **Assets/Scripts/Persistent/README_Persistent.md**: TimerManagerの使い方
