# Facility自動生成機能 - タイマー統合手順書

## 概要
Facilityの自動生成機能は現在Time.timeを使用していますが、将来TimerManagerと統合する必要があります。
このドキュメントではその統合手順を詳しく説明します。

## 現在の実装状況

### 実装済み機能
- ✅ 生成設定データ構造 (ProductionConfig, ItemProduction, ProductionState)
- ✅ FacilityDataへの生成設定の追加
- ✅ Facilityクラスの拡張（生成設定・状態管理）
- ✅ FacilityProductionControllerによる自動生成管理
- ✅ お金とアイテムの生成切り替え
- ✅ 生成間隔の設定
- ✅ 生成統計の記録

### 未実装（タイマー統合待ち）
- ⏳ TimerManagerとの連携
- ⏳ ゲーム時間に基づく生成管理
- ⏳ タイマー一時停止時の生成停止
- ⏳ タイマー終了時の処理

## タイマー統合手順

### ステップ1: TimerManagerの準備確認

統合前に、以下が実装されていることを確認：
- [x] `Assets/Scripts/Persistent/Managers/TimerManager.cs` が存在
- [x] TimerManagerがシングルトンとして実装されている
- [x] `CheckInterval(float interval, ref float lastCheckTime)` メソッドが存在
- [x] イベント: OnTimerPausedChanged, OnTimerFinished が定義されている

### ステップ2: FacilityProductionControllerの修正

#### 2.1 フィールドの修正

**変更前:**
```csharp
private void Update()
{
    // 現在は通常のTime.timeを使用（タイマー実装後に置き換え）
    UpdateFacilityProduction();
}
```

**変更後:**
```csharp
private void Start()
{
    // TimerManagerのイベントに登録
    if (TimerManager.Instance != null)
    {
        TimerManager.Instance.OnTimerPausedChanged += OnTimerPausedChanged;
        TimerManager.Instance.OnTimerFinished += OnTimerFinished;
        TimerManager.Instance.OnTimerStarted += OnTimerStarted;
    }
    else
    {
        Debug.LogWarning("TimerManager not found! Using fallback Time.time.");
    }
}

private void OnDestroy()
{
    // イベント解除
    if (TimerManager.Instance != null)
    {
        TimerManager.Instance.OnTimerPausedChanged -= OnTimerPausedChanged;
        TimerManager.Instance.OnTimerFinished -= OnTimerFinished;
        TimerManager.Instance.OnTimerStarted -= OnTimerStarted;
    }
}

private void Update()
{
    // TimerManagerが利用可能な場合はそちらを使用
    if (TimerManager.Instance != null && TimerManager.Instance.IsRunning)
    {
        UpdateFacilityProductionWithTimer();
    }
    else
    {
        // フォールバック: Time.timeを使用
        UpdateFacilityProduction();
    }
}
```

#### 2.2 TimerManager連携メソッドの追加

**FacilityProductionController.csに追加:**

```csharp
/// <summary>
/// TimerManagerを使用した生成更新
/// </summary>
private void UpdateFacilityProductionWithTimer()
{
    foreach (var facility in managedFacilities)
    {
        // 解放されていない、または設定が無効な場合はスキップ
        if (!facility.isUnlocked || !facility.productionConfig.IsValid())
            continue;

        // 一時停止中はスキップ
        if (facility.productionState.isPaused)
            continue;

        // TimerManagerのCheckIntervalを使用
        if (TimerManager.Instance.CheckInterval(
            facility.productionConfig.productionInterval,
            ref facility.productionState.lastProductionTime))
        {
            ProduceFacilityResources(facility);
        }
    }
}

/// <summary>
/// タイマー一時停止/再開時の処理
/// </summary>
private void OnTimerPausedChanged(bool isPaused)
{
    SetAllProductionPaused(isPaused);
    
    if (showDebugLog)
    {
        Debug.Log($"Timer {(isPaused ? "paused" : "resumed")} - All facility production {(isPaused ? "stopped" : "started")}.");
    }
}

/// <summary>
/// タイマー終了時の処理
/// </summary>
private void OnTimerFinished()
{
    SetAllProductionPaused(true);
    
    if (showDebugLog)
    {
        Debug.Log("Timer finished - All facility production stopped.");
        ShowAllProductionStats();
    }
}

/// <summary>
/// タイマー開始時の処理
/// </summary>
private void OnTimerStarted()
{
    // すべてのFacilityの最終生成時刻をリセット
    foreach (var facility in managedFacilities)
    {
        facility.productionState.lastProductionTime = TimerManager.Instance.CurrentTime;
    }
    
    if (showDebugLog)
    {
        Debug.Log("Timer started - Facility production initialized.");
    }
}
```

#### 2.3 RegisterFacilityメソッドの修正

**変更前:**
```csharp
managedFacilities.Add(facility);
facility.productionState.lastProductionTime = Time.time;
```

**変更後:**
```csharp
managedFacilities.Add(facility);

// TimerManagerがあればそちらを使用
if (TimerManager.Instance != null)
{
    facility.productionState.lastProductionTime = TimerManager.Instance.CurrentTime;
}
else
{
    facility.productionState.lastProductionTime = Time.time;
}
```

#### 2.4 ManualProduceResourcesメソッドの修正

**変更前:**
```csharp
ProduceFacilityResources(facility);
facility.productionState.lastProductionTime = Time.time;
```

**変更後:**
```csharp
ProduceFacilityResources(facility);

// TimerManagerがあればそちらを使用
if (TimerManager.Instance != null)
{
    facility.productionState.lastProductionTime = TimerManager.Instance.CurrentTime;
}
else
{
    facility.productionState.lastProductionTime = Time.time;
}
```

### ステップ3: ProductionStateの修正（オプション）

タイマーの経過時間を記録したい場合、ProductionStateに追加フィールドを検討：

```csharp
[Serializable]
public class ProductionState
{
    // 既存のフィールド...
    
    [Tooltip("最初に生成を開始したゲーム内時刻")]
    public float firstProductionGameTime;
    
    [Tooltip("ゲーム内時間での累計生成時間")]
    public float totalProductionGameTime;
}
```

### ステップ4: GameDatabaseとの統合

ProduceFacilityResourcesメソッド内のTODOコメントを実装：

```csharp
// お金の生成
if (facility.productionConfig.ProducesMoney())
{
    moneyProduced = facility.productionConfig.moneyAmount;
    
    // GameDatabaseにお金を追加
    if (GameDatabase.Instance != null)
    {
        GameDatabase.Instance.AddMoney(moneyProduced);
    }
    
    if (showDebugLog)
    {
        Debug.Log($"[{facility.facilityName}] お金を生成: {moneyProduced}");
    }
}

// アイテムの生成
if (facility.productionConfig.ProducesItems())
{
    foreach (var itemProd in facility.productionConfig.itemProductions)
    {
        string itemId = itemProd.GetItemId();
        int quantity = itemProd.quantity;

        // GameDatabaseにアイテムを追加
        if (GameDatabase.Instance != null)
        {
            GameDatabase.Instance.AddItem(itemId, quantity);
        }

        itemsProduced[itemId] = quantity;

        if (showDebugLog)
        {
            Debug.Log($"[{facility.facilityName}] アイテムを生成: {itemId} x{quantity}");
        }
    }
}
```

### ステップ5: テスト手順

1. **単体テスト**
   - TimerManagerなしで従来通り動作するか確認
   - TimerManagerありで動作するか確認
   - タイマー一時停止時に生成が止まるか確認
   - タイマー終了時に統計が表示されるか確認

2. **統合テスト**
   - 複数Facilityの同時生成動作確認
   - シーン遷移後も生成が継続するか確認
   - GameDatabaseへの正しい値の追加を確認

3. **パフォーマンステスト**
   - 多数のFacilityを登録した場合の動作確認
   - 生成間隔が短い場合の動作確認

## チェックリスト

統合作業を行う際の確認項目：

- [ ] TimerManager.Instanceの存在確認コードを追加
- [ ] OnTimerPausedChangedイベントハンドラーを実装
- [ ] OnTimerFinishedイベントハンドラーを実装
- [ ] OnTimerStartedイベントハンドラーを実装
- [ ] CheckInterval()メソッドを使用するよう変更
- [ ] Time.timeへのフォールバック処理を実装
- [ ] OnDestroyでイベント解除を実装
- [ ] GameDatabase連携コードのTODOを実装
- [ ] 単体テストを実施
- [ ] 統合テストを実施

## 注意事項

1. **後方互換性**: TimerManagerがない環境でも動作するようフォールバック処理を実装
2. **イベント解除**: OnDestroyで必ずイベント解除を行う（メモリリーク防止）
3. **Null チェック**: TimerManager.Instanceは常にnullチェックを行う
4. **生成時刻の管理**: lastProductionTimeはTimerManager.CurrentTimeを使用（Time.timeではない）
5. **デバッグログ**: 統合後もデバッグログを残し、動作確認を容易にする

## トラブルシューティング

### 生成が動作しない場合
1. TimerManagerが存在し、StartTimer()が呼ばれているか確認
2. Facilityが解放されているか確認（isUnlocked = true）
3. ProductionConfig.enableAutoProduction が true か確認
4. タイマーが一時停止していないか確認

### 生成間隔がおかしい場合
1. CheckInterval()の第2引数が参照渡し（ref）になっているか確認
2. lastProductionTimeが正しく初期化されているか確認
3. TimerManager.CurrentTimeとTime.timeを混在させていないか確認

### イベントが発火しない場合
1. Startでイベント登録を行っているか確認
2. TimerManagerが複数存在していないか確認（シングルトン）
3. OnDestroyでイベント解除漏れがないか確認

## ファイル一覧

統合に関連するファイル：
- `Assets/Scripts/FacilityProduction.cs` - 生成設定データ構造
- `Assets/Scripts/Facility.cs` - Facilityクラス本体
- `Assets/Scripts/FacilityData.cs` - FacilityのScriptableObject
- `Assets/Scripts/FacilityProductionController.cs` - 生成コントローラー（要修正）
- `Assets/Scripts/Persistent/Managers/TimerManager.cs` - タイマー管理（統合先）
- `Assets/Scripts/Persistent/Managers/TimerBasedFacility.cs` - 連携の参考実装

## 参考実装

TimerBasedFacility.csに類似の実装例があります：
```csharp
private void CheckProduction()
{
    if (TimerManager.Instance.CheckInterval(productionInterval, ref lastProductionTime))
    {
        ProduceItem();
    }
}
```

このパターンをFacilityProductionControllerに適用してください。
