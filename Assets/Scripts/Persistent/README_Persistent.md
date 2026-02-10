# 永続表示システム (Persistent Display System)

## 概要
シーンを跨いで常に表示され続ける情報（タイマー、総資産など）を管理するシステムです。
他のスクリプトと独立しており、Facilityなど他の機能からも簡単にアクセスできます。

## ディレクトリ構造

```
Assets/Scripts/Persistent/
├── Managers/
│   ├── TimerManager.cs              # タイマー管理（シングルトン）
│   └── TimerBasedFacility.cs        # タイマー連携Facilityの例
├── UI/
│   ├── PersistentUIManager.cs       # 永続UI管理（シングルトン）
│   ├── TimerDisplay.cs              # タイマー表示UI
│   ├── PersistentInfoDisplay.cs     # 情報表示の基底クラス
│   └── TotalAssetsDisplay.cs        # 総資産表示の例
└── README_Persistent.md             # このファイル
```

## 主要コンポーネント

### 1. TimerManager
ゲーム全体のタイマーを管理するシングルトンクラス。

**特徴:**
- シーン間で永続化 (DontDestroyOnLoad)
- どこからでもアクセス可能なシングルトン
- カウントダウン/カウントアップの両モード対応
- イベント駆動型（タイマー更新、終了などのイベント）

**主な機能:**
- `StartTimer()` - タイマー開始
- `StopTimer()` - タイマー停止
- `TogglePause()` - 一時停止/再開
- `ResetTimer()` - リセット
- `AddTime(float)` - 時間追加（ボーナスタイム）
- `GetFormattedTime()` - MM:SS形式の文字列取得
- `CheckInterval()` - 一定間隔チェック（Facility用）

**使用例:**
```csharp
// タイマー開始
TimerManager.Instance.StartTimer();

// 残り時間を取得
float remaining = TimerManager.Instance.RemainingTime;

// 時間を追加
TimerManager.Instance.AddTime(30f); // 30秒追加

// フォーマット済み時間を取得
string timeStr = TimerManager.Instance.GetFormattedTime(); // "05:30"
```

### 2. PersistentUIManager
常に表示されるUI要素を統合管理するマネージャー。

**特徴:**
- 専用のCanvasを自動生成
- シーン間で永続化
- 複数の表示要素を登録・管理
- 表示順序の制御

**主な機能:**
- `RegisterDisplay(key, obj)` - 表示要素を登録
- `UnregisterDisplay(key)` - 登録解除
- `ToggleDisplay(key, visible)` - 表示/非表示切り替え
- `ToggleTimerDisplay(visible)` - タイマー表示切り替え

### 3. TimerDisplay
タイマーを表示するUIコンポーネント。

**特徴:**
- TimerManagerのイベントに自動連携
- 警告色表示（時間切れ間近）
- 点滅エフェクト
- 2つの表示フォーマット（MM:SS / HH:MM:SS）

### 4. PersistentInfoDisplay (抽象クラス)
永続表示される情報の基底クラス。

**使い方:**
このクラスを継承して、独自の表示要素を作成します。

```csharp
public class MyCustomDisplay : PersistentInfoDisplay
{
    protected override void UpdateDisplay()
    {
        // 表示内容を更新
        SetText("カスタム情報");
    }

    protected override string GetDisplayKey()
    {
        return "MyCustomDisplay";
    }
}
```

### 5. TimerBasedFacility
Facilityからタイマーを利用する例。

**特徴:**
- 一定時間ごとのアイテム生成
- TimerManagerとの連携
- 自動/手動生成の切り替え

**Facilityでの使用例:**
```csharp
// Facilityスクリプト内で
public class MyFacility : TimerBasedFacility
{
    protected override void ProduceItem()
    {
        base.ProduceItem();
        
        // 実際のアイテム生成処理
        GameDatabase.Instance.AddItem(new Item());
    }
}
```

**直接TimerManagerを使う場合:**
```csharp
public class MyFacility : MonoBehaviour
{
    private float lastCheckTime;
    [SerializeField] private float interval = 5f;

    void Update()
    {
        if (TimerManager.Instance.CheckInterval(interval, ref lastCheckTime))
        {
            // 5秒ごとに実行される処理
            ProduceItem();
        }
    }
}
```

## セットアップ手順

### 基本セットアップ
1. 空のGameObjectを作成し、`TimerManager`スクリプトをアタッチ
2. 空のGameObjectを作成し、`PersistentUIManager`スクリプトをアタッチ
3. 実行すると自動的に永続Canvasが生成されます

### タイマー表示の追加
1. UI > Text - TextMeshPro を作成
2. `TimerDisplay`スクリプトをアタッチ
3. TextMeshProUGUIコンポーネントをTimerDisplayの`Timer Text`にドラッグ
4. 表示設定を調整（警告色、点滅など）

### 総資産表示の追加（例）
1. UI > Text - TextMeshPro を作成
2. `TotalAssetsDisplay`スクリプトをアタッチ
3. TextMeshProUGUIコンポーネントを設定
4. 自動的にPersistentUIManagerに登録されます

## Facilityとの連携

### 方法1: TimerBasedFacilityを継承
```csharp
public class ResourceFacility : TimerBasedFacility
{
    [SerializeField] private ItemData resourceType;

    protected override void ProduceItem()
    {
        base.ProduceItem();
        
        // カスタム生成ロジック
        GameDatabase.Instance.AddItem(resourceType.CreateItem());
    }
}
```

### 方法2: 直接TimerManagerにアクセス
```csharp
public class CustomFacility : MonoBehaviour
{
    private float lastProductionTime;

    void Update()
    {
        // 10秒ごとにチェック
        if (TimerManager.Instance.CheckInterval(10f, ref lastProductionTime))
        {
            ProduceResource();
        }
    }

    void ProduceResource()
    {
        // 生成処理
    }
}
```

## 将来の拡張例

### カスタム表示要素の追加
```csharp
public class PlayerLevelDisplay : PersistentInfoDisplay
{
    protected override void UpdateDisplay()
    {
        int level = PlayerData.Instance.Level;
        SetText($"Lv.{level}");
    }

    protected override string GetDisplayKey()
    {
        return "PlayerLevel";
    }
}
```

### 複数の情報を一括管理
```csharp
// PersistentUIManagerに登録
PersistentUIManager.Instance.RegisterDisplay("Assets", assetsDisplayObj);
PersistentUIManager.Instance.RegisterDisplay("Level", levelDisplayObj);
PersistentUIManager.Instance.RegisterDisplay("Score", scoreDisplayObj);

// まとめて非表示
PersistentUIManager.Instance.ToggleAllDisplays(false);
```

## イベント活用

TimerManagerは各種イベントを提供しています：

```csharp
void Start()
{
    TimerManager.Instance.OnTimerUpdated += OnTimerUpdate;
    TimerManager.Instance.OnTimerFinished += OnTimerFinish;
    TimerManager.Instance.OnTimerStarted += OnTimerStart;
    TimerManager.Instance.OnTimerStopped += OnTimerStop;
}

void OnTimerUpdate(float currentTime)
{
    Debug.Log($"経過時間: {currentTime}秒");
}

void OnTimerFinish()
{
    Debug.Log("時間切れ！");
    // ゲームオーバー処理など
}
```

## 注意事項

1. **シングルトン**: TimerManagerとPersistentUIManagerはシングルトンなので、シーンに1つだけ配置してください
2. **DontDestroyOnLoad**: これらのオブジェクトはシーン遷移後も残り続けます
3. **イベント登録解除**: OnDestroyでイベントを解除することを忘れずに
4. **Canvasの表示順序**: 他のUIと重ならないよう、Sort Orderを調整できます

## トラブルシューティング

### タイマーが表示されない
- PersistentUIManagerが正しく初期化されているか確認
- TimerDisplayのTextMeshProUGUIが設定されているか確認

### Facilityでタイマーが機能しない
- TimerManagerが存在し、StartTimer()が呼ばれているか確認
- CheckIntervalの引数で渡す`lastCheckTime`が参照渡しになっているか確認

### シーン遷移後に表示が消える
- DontDestroyOnLoadが正しく適用されているか確認
- PersistentUIManagerのCanvasが正しく設定されているか確認
