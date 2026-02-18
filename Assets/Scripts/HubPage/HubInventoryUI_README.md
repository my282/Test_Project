# HubPage インベントリUI システム

## 概要
HubPageでアイテムインベントリを自動更新表示するシステムです。GameDatabaseのアイテムが変更されると即座にUIが更新されます。

## 作成されたスクリプト

### 1. GameDatabase.cs (修正)
- `OnItemsChanged` イベントを追加
- `AddItem()` と `RemoveItem()` でイベントを発火

### 2. HubInventoryUIManager.cs (新規)
インベントリUIの管理を行うメインスクリプト

**主な機能:**
- GameDatabaseの変更を自動検知
- アイテムスロットを動的に生成/更新
- グリッドレイアウト対応
- アイテムタイプによるフィルタリング
- 柔軟な配置設定

### 3. InventoryItemSlot.cs (新規)
個別のアイテムスロットUIを管理

**表示内容:**
- アイコン画像
- アイテム名
- 所持数量

## セットアップ方法

### 1. Hierarchyでの設定

1. HubPageシーンで空のGameObjectを作成し、"InventoryUI"と命名
2. その子に、空のGameObjectを作成し、"InventoryContainer"と命名
3. InventoryContainerにRectTransformを設定（配置する場所とサイズを調整）

### 2. HubInventoryUIManagerの設定

1. InventoryUI オブジェクトに `HubInventoryUIManager` コンポーネントを追加
2. Inspector で以下を設定:
   - **Inventory Container**: InventoryContainer オブジェクトをドラッグ＆ドロップ
   - **Item Slot Prefab**: （オプション）カスタムプレハブを使用する場合は設定
   - **Use Grid Layout**: グリッド表示の場合はチェック
   - **Columns**: 1行あたりの列数
   - **Cell Size**: 各スロットのサイズ（例: 150x150）
   - **Spacing**: スロット間の間隔（例: 10x10）
   - **Default Icon**: アイテムにアイコンがない場合のデフォルト画像

### 3. 配置のカスタマイズ

**InventoryContainer の RectTransform を調整することで配置を変更:**

- **画面上部に横並び**: Anchor を Top Center に設定
- **画面右側にリスト**: Anchor を Right Center に設定、Columns を 1 に
- **画面下部にグリッド**: Anchor を Bottom Center に設定
- **中央にパネル**: Anchor を Middle Center に設定

## 使用例

### 基本的な使用
```csharp
// アイテムを追加すると自動的にUIが更新される
GameDatabase.Instance.AddItem("item1", "回復ポーション", "HPを回復する", 5, ItemType.Consumable);

// アイテムを削除すると自動的にUIから消える
GameDatabase.Instance.RemoveItem("item1", 2);
```

### フィルタリング
```csharp
// 特定のタイプのアイテムのみ表示
HubInventoryUIManager inventoryUI = FindObjectOfType<HubInventoryUIManager>();
inventoryUI.SetFilterType(ItemType.Consumable); // 消耗品のみ表示

// フィルターを解除
inventoryUI.ClearFilter();
```

### 手動更新（通常は不要）
```csharp
// イベントシステムが自動的に更新するため通常は不要ですが、
// 必要に応じて明示的に更新可能
HubInventoryUIManager inventoryUI = FindObjectOfType<HubInventoryUIManager>();
inventoryUI.RefreshInventory();
```

## カスタマイズ

### カスタムアイテムスロットプレハブの作成

1. 新しいGameObjectを作成
2. 以下の子オブジェクトを作成:
   - `Icon` (Image コンポーネント)
   - `Name` (TextMeshProUGUI コンポーネント)
   - `Quantity` (TextMeshProUGUI コンポーネント)
3. `InventoryItemSlot` コンポーネントを追加
4. プレハブとして保存
5. HubInventoryUIManagerの"Item Slot Prefab"に設定

### アイテムクリック時の動作を拡張

`InventoryItemSlot.cs` の `OnPointerClick` メソッドを編集して、
クリック時の詳細表示などの機能を追加できます。

## 注意事項

- GameDatabaseのシングルトンインスタンスが存在している必要があります
- TextMeshProを使用しているため、プロジェクトにインポートされている必要があります
- アイテムにアイコンを設定する場合は、Item.icon に Sprite を設定してください

## トラブルシューティング

**UIが更新されない:**
- GameDatabase.Instance が正しく初期化されているか確認
- HubInventoryUIManagerの OnEnable/OnDisable でイベント登録されているか確認

**アイテムが表示されない:**
- InventoryContainer が正しく設定されているか確認
- Canvas 内に配置されているか確認
- RectTransform のサイズが適切か確認

**レイアウトが崩れる:**
- Cell Size と Spacing の値を調整
- InventoryContainer の RectTransform サイズを確認
- Columns の数を調整
