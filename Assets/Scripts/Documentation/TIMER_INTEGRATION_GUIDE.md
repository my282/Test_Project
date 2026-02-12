# Facility自動生成機能 - TimerManager統合完了報告書

## 統合状況

**✅ 統合完了日**: 2026年2月12日

Facilityの自動生成機能とTimerManagerの統合が完了しました。
このドキュメントでは統合内容と今後のメンテナンスガイドを説明します。

## 実装済み機能

### 基本機能
- ✅ 生成設定データ構造 (ProductionConfig, ItemProduction, ProductionState)
- ✅ FacilityDataへの生成設定の追加
- ✅ Facilityクラスの拡張（生成設定・状態管理）
- ✅ FacilityProductionControllerによる自動生成管理
- ✅ お金とアイテムの生成切り替え
- ✅ 生成間隔の設定
- ✅ 生成統計の記録

### TimerManager統合機能
- ✅ TimerManagerとの連携
- ✅ ゲーム時間に基づく生成管理
- ✅ タイマー一時停止時の生成停止
- ✅ タイマー終了時の処理
- ✅ Time.timeへのフォールバック機能

## 統合内容の詳細

## 統合内容の詳細

### 1. FacilityProductionController.cs の変更

#### 追加されたフィールド
```csharp
// TimerManagerが利用可能かどうか
private bool useTimerManager = false;
```

#### Start() メソッドの追加
TimerManagerの初期化とイベント登録：
```csharp
private void Start()
{
    // TimerManagerのイベントに登録
    if (TimerManager.Instance != null)
    {
        useTimerManager = true;
        TimerManager.Instance.OnTimerPausedChanged += OnTimerPausedChanged;
        TimerManager.Instance.OnTimerFinished += OnTimerFinished;
        TimerManager.Instance.OnTimerStarted += OnTimerStarted;

        if (showDebugLog)
        {
            Debug.Log("FacilityProductionController: TimerManagerと統合しました");
        }
    }
    else
    {
        Debug.LogWarning("FacilityProductionController: TimerManagerが見つかりません。Time.timeをフォールバックとして使用します。");
    }
}
```

#### OnDestroy() メソッドの追加
イベントのクリーンアップ：
```csharp
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
```

#### Update() メソッドの修正
TimerManagerとTime.timeの切り替え：
```csharp
private void Update()
{
    // TimerManagerが利用可能な場合はそちらを使用
    if (useTimerManager && TimerManager.Instance != null && TimerManager.Instance.IsRunning)
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

#### UpdateFacilityProductionWithTimer() メソッドの追加
TimerManagerを使用した新しい生成更新ロジック：
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

        // TimerManagerのCheckIntervalメソッドを使用
        if (TimerManager.Instance.CheckInterval(
            facility.productionConfig.productionInterval,
            ref facility.productionState.lastProductionTime))
        {
            ProduceFacilityResources(facility);
        }
    }
}
```

#### TimerManagerイベントハンドラーの追加

**OnTimerStarted()** - タイマー開始時の処理：
```csharp
private void OnTimerStarted()
{
    if (showDebugLog)
    {
        Debug.Log("FacilityProductionController: タイマーが開始されました。生成を開始します。");
    }

    // すべてのFacilityの一時停止を解除
    SetAllProductionPaused(false);

    // lastProductionTimeをTimerManagerの時間で初期化
    foreach (var facility in managedFacilities)
    {
        facility.productionState.lastProductionTime = TimerManager.Instance.CurrentTime;
    }
}
```

**OnTimerPausedChanged()** - 一時停止状態変更時の処理：
```csharp
private void OnTimerPausedChanged(bool isPaused)
{
    if (showDebugLog)
    {
        Debug.Log($"FacilityProductionController: タイマーが{(isPaused ? "一時停止" : "再開")}されました。");
    }

    // すべてのFacilityの生成を一時停止/再開
    SetAllProductionPaused(isPaused);
}
```

**OnTimerFinished()** - タイマー終了時の処理：
```csharp
private void OnTimerFinished()
{
    if (showDebugLog)
    {
        Debug.Log("FacilityProductionController: タイマーが終了しました。生成を停止します。");
        ShowAllProductionStats();
    }

    // すべての生成を停止
    SetAllProductionPaused(true);
}
```

### 2. FacilityProduction.cs の変更

TODOコメントを更新し、統合完了を明記：
```csharp
// NOTE: TimerManager統合完了
// FacilityProductionControllerでTimerManagerと統合済み
// - CheckInterval()メソッドを使用した時間チェック
// - OnTimerFinishedイベントでゲーム終了時の処理
// - OnTimerPausedChangedイベントで一時停止時の生成制御
// - OnTimerStartedイベントで開始時の初期化
```

## 動作仕様

### TimerManagerが利用可能な場合
1. タイマー開始時に全Facilityの`lastProductionTime`を初期化
2. `TimerManager.CheckInterval()`を使用して生成間隔を管理
3. タイマー一時停止時に全Facilityの生成を停止
4. タイマー終了時に生成を停止し、統計を表示

### TimerManagerが利用できない場合（フォールバック）
1. 従来通り`Time.time`を使用
2. 手動での一時停止制御のみ可能
3. タイマーイベントは受け取らない

## テスト手順

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

## 今後の作業

### GameDatabase連携の実装

`ProduceFacilityResources()`メソッド内のTODOコメントを実装する必要があります：

```csharp
// お金の生成
if (facility.productionConfig.ProducesMoney())
{
    moneyProduced = facility.productionConfig.moneyAmount;
    
    // TODO: GameDatabaseにお金を追加
    // if (GameDatabase.Instance != null)
    // {
    //     GameDatabase.Instance.AddMoney(moneyProduced);
    // }
    
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

        // TODO: GameDatabaseにアイテムを追加
        // if (GameDatabase.Instance != null)
        // {
        //     GameDatabase.Instance.AddItem(itemId, quantity);
        // }

        itemsProduced[itemId] = quantity;

        if (showDebugLog)
        {
            Debug.Log($"[{facility.facilityName}] アイテムを生成: {itemId} x{quantity}");
        }
    }
}
```

### オプション機能の検討

必要に応じて以下の機能を追加することを検討：

1. **ProductionStateの拡張**
   - ゲーム内時間での累計生成時間の記録
   - 最初の生成開始時刻の記録

2. **RegisterFacility/ManualProduceResourcesの改善**
   - TimerManagerの時間を優先的に使用するよう修正
   - 現在はTime.timeで初期化されている

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
テスト手順

統合機能の動作確認：

1. **TimerManagerありでの動作確認**
   - [ ] TimerManager.StartTimer()を呼び出し、タイマーを開始
   - [ ] Facilityが設定した間隔で生成を行うか確認
   - [ ] デバッグログで「TimerManagerと統合しました」が表示されることを確認

2. **一時停止機能の確認**
   - [ ] TimerManager.TogglePause()でタイマーを一時停止
   - [ ] すべてのFacilityの生成が停止することを確認
   - [ ] 再開時に生成が再開されることを確認

3. **タイマー終了処理の確認**
   - [ ] タイマーが終了したとき、すべての生成が停止することを確認
   - [ ] 生成統計が表示されることを確認

4. **フォールバック機能の確認**
   - [ ] TimerManagerオブジェクトを削除
   - [ ] Time.timeを使用したフォールバック処理が動作することを確認
   - [ ] 警告ログ「TimerManagerが見つかりません」が表示されることを確認

5. **複数Facilityの動作確認**
   - [ ] 異なる生成間隔を持つ複数のFacilityを登録
   - [ ] それぞれが正しい間隔で生成を行うか確認

## 完了チェックリスト

統合作業の確認項目：

- [x] TimerManager.Instanceの存在確認コードを追加
- [x] OnTimerPausedChangedイベントハンドラーを実装
- [x] OnTimerFinishedイベントハンドラーを実装
- [x] OnTimerStartedイベントハンドラーを実装
- [x] CheckInterval()メソッドを使用するよう変更
- [x] Time.timeへのフォールバック処理を実装
- [x] OnDestroyでイベント解除を実装
- [ ] GameDatabase連携コードのTODO実装（今後の作業）