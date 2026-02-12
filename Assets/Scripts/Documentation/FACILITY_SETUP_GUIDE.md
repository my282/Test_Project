# Facility自動生成機能 - セットアップガイド

**最終更新**: 2026年2月12日  
**対象**: 初めて設備を設定する方向け

このガイドでは、既存の設備（FacilityA など）に自動生成機能を追加する実際の手順を説明します。

---

## 前提条件

以下のファイルが既に存在していること：
- ✅ FacilityData（例: FacilityA.asset）
- ✅ GameDatabase
- ✅ MasterDatabase（Resources フォルダ内）

---

## ステップ1: アイテムデータの作成

### 1-1. ItemData を作成

```
1. Project ウィンドウで右クリック
   Create > Game > Item Data

2. ファイル名を設定（例: itemA）

3. Inspector で設定:
   ┌─────────────────────────────┐
   │ Item Id: "itemA"           │
   │ Item Name: "アイテムA"      │
   │ Description: "説明文"       │
   │ Type: Material (適切な型)   │
   │ Max Stack: 99               │
   └─────────────────────────────┘
```

### 1-2. MasterDatabase に登録

```
1. Project > Assets/Resources/MasterDatabase を選択

2. Inspector の「All Items」セクション
   
3. Size を +1 増やす

4. 作成した ItemData をドラッグ＆ドロップ

5. Ctrl+S で保存
```

**重要**: MasterDatabase に登録しないと、生成時にアイテムが見つからないエラーが出ます。

---

## ステップ2: FacilityData の設定

### 2-1. FacilityData を開く

```
Project > Assets/GameData/Facilities/FacilityA.asset
```

### 2-2. 自動生成設定（Inspector）

#### パターンA: アイテムを生成する場合

```
【自動生成設定】
├─ Production Type: Item
├─ Enable Auto Production: ✓ （必ずチェック）
├─ Production Interval: 10.0
└─ Item Productions:
    Size: 1
    Element 0:
      ├─ Item Id: "itemA"
      ├─ Item Data: (作成したItemDataをドラッグ)
      └─ Quantity: 1
```

#### パターンB: お金を生成する場合

```
【自動生成設定】
├─ Production Type: Money
├─ Enable Auto Production: ✓
├─ Production Interval: 10.0
└─ Money Amount: 100
```

#### パターンC: お金とアイテム両方

```
【自動生成設定】
├─ Production Type: Both
├─ Enable Auto Production: ✓
├─ Production Interval: 15.0
├─ Money Amount: 50
└─ Item Productions:
    Size: 2
    Element 0:
      └─ Item Id: "itemA", Quantity: 1
    Element 1:
      └─ Item Id: "itemB", Quantity: 2
```

**📌 チェックポイント**
- [ ] Enable Auto Production が ON
- [ ] Production Type が None **以外**
- [ ] Production Interval が 0 より大きい
- [ ] Item の場合、Item Productions の Size が 1 以上

---

## ステップ3: 解放スクリプトの作成

既存の設備解放スクリプト（例: buildFacilityA.cs）を修正します。

### 3-1. スクリプトを開く

```
Assets/Scripts/tests/buildFacilityA.cs
```

### 3-2. 自動生成登録コードを追加

```csharp
using UnityEngine;

public class build_FacilityA : MonoBehaviour
{
    public void build()
    {
        // 設備を解放
        GameDatabase.Instance.UnlockFacilityWithCost("FacilityA");
        
        // 自動生成を登録
        RegisterAutoProduction();
        
        // TimerManagerを開始
        StartTimerIfNeeded();
    }
    
    void RegisterAutoProduction()
    {
        Facility facilityA = GameDatabase.Instance.GetFacility("FacilityA");
        
        if (facilityA != null && facilityA.isUnlocked)
        {
            if (facilityA.productionConfig.enableAutoProduction)
            {
                if (FacilityProductionController.Instance != null)
                {
                    FacilityProductionController.Instance.RegisterFacility(facilityA);
                    Debug.Log("✅ FacilityAの自動生成を開始しました！");
                }
                else
                {
                    // 自動作成
                    CreateProductionController();
                }
            }
        }
    }
    
    void StartTimerIfNeeded()
    {
        if (TimerManager.Instance != null && !TimerManager.Instance.IsRunning)
        {
            TimerManager.Instance.StartTimer();
            Debug.Log("✅ TimerManagerを開始しました");
        }
    }
    
    void CreateProductionController()
    {
        GameObject go = new GameObject("FacilityProductionController");
        go.AddComponent<FacilityProductionController>();
        Debug.Log("✅ FacilityProductionControllerを自動作成しました");
        
        // 再登録
        RegisterAutoProduction();
    }
}
```

---

## ステップ4: シーンの確認

### 4-1. 必須オブジェクトの確認

Hierarchy に以下が存在することを確認：

```
Hierarchy:
  ├─ GameDatabase          ← DontDestroyOnLoad
  ├─ TimerManager          ← DontDestroyOnLoad（任意）
  └─ (build()を実行すると FacilityProductionController が自動作成されます)
```

### 4-2. GameDatabase の確認

```
1. Hierarchy > GameDatabase を選択

2. Inspector で Script が正しくアタッチされているか確認

3. Player Money, Items, Facilities が初期化されているか確認
```

---

## ステップ5: 動作確認

### 5-1. 実行

```
1. Play ボタンを押す

2. build() メソッドを実行
   （UIボタンまたはスクリプトから）
```

### 5-2. コンソールログを確認

**正常な場合:**
```
✅ FacilityAの自動生成を開始しました！
✅ TimerManagerを開始しました
Facility 'FacilityA' registered for production.
  - Production Type: Item
  - Interval: 10s
  - Using Timer: TimerManager
  - IsValid: True

（10秒後）
[FacilityA] アイテムを生成: itemA x1
新しいアイテム「アイテムA」を1個取得しました。
[FacilityA] 生成完了 (累計: 1回)
```

**エラーがある場合:**
```
❌ ItemData 'itemA' が見つかりません。
   → MasterDatabase に ItemData を登録

❌ FacilityAの自動生成は無効になっています
   → FacilityData で Enable Auto Production をON

❌ GameDatabaseが見つかりません
   → シーンに GameDatabase オブジェクトを配置
```

---

## トラブルシューティング

### 問題1: 生成されない

#### デバッグコマンドを実行

```csharp
// build_FacilityA.cs に追加済み
[ContextMenu("自動生成状態を確認")]
void DebugProductionStatus()
{
    // 詳細な状態をコンソールに出力
}
```

**使い方:**
1. Hierarchy で build_FacilityA オブジェクトを選択
2. Inspector の右上 ⋮ メニュー > "自動生成状態を確認"
3. コンソールで状態を確認

#### 確認項目

| 項目 | 期待値 | 対処 |
|------|--------|------|
| isUnlocked | True | build() を実行 |
| enableAutoProduction | True | FacilityData で ON に |
| IsValid | True | 設定を確認 |
| IsRunning (Timer) | True | TimerManager.StartTimer() |
| isPaused | False | SetAllProductionPaused(false) |

---

### 問題2: アイテムが追加されない

**原因**: MasterDatabase に ItemData が未登録

**確認方法:**
```
1. Project > Assets/Resources/MasterDatabase

2. Inspector > All Items に該当アイテムがあるか確認

3. なければ追加
```

---

### 問題3: お金が増えない

**原因**: Production Type が Item になっている

**対処:**
```
FacilityData > Production Type を Money または Both に変更
```

---

## 他の設備にも適用する場合

### 汎用スクリプトの作成

```csharp
using UnityEngine;

/// <summary>
/// 任意の設備を解放して自動生成を開始
/// </summary>
public class FacilityUnlocker : MonoBehaviour
{
    public void UnlockAndStartProduction(string facilityId)
    {
        // 解放
        bool unlocked = GameDatabase.Instance.UnlockFacilityWithCost(facilityId);
        if (!unlocked) return;
        
        // 取得
        Facility facility = GameDatabase.Instance.GetFacility(facilityId);
        if (facility == null) return;
        
        // 登録
        if (facility.productionConfig.enableAutoProduction)
        {
            FacilityProductionController.Instance?.RegisterFacility(facility);
            Debug.Log($"✅ {facility.facilityName} の自動生成を開始");
        }
        
        // タイマー開始
        if (TimerManager.Instance != null && !TimerManager.Instance.IsRunning)
        {
            TimerManager.Instance.StartTimer();
        }
    }
    
    // UIボタンから呼び出す用
    public void UnlockFacilityA() => UnlockAndStartProduction("FacilityA");
    public void UnlockFacilityB() => UnlockAndStartProduction("FacilityB");
    public void UnlockFacilityC() => UnlockAndStartProduction("FacilityC");
}
```

**使い方:**
1. 空のGameObject に FacilityUnlocker をアタッチ
2. UIボタンの OnClick に UnlockFacilityA などを設定
3. どの設備でも同じコードで対応可能

---

## まとめ

### 最小限の手順

1. ✅ **ItemData を作成** → MasterDatabase に登録
2. ✅ **FacilityData を設定** → Enable Auto Production ON
3. ✅ **解放スクリプトに登録処理を追加**
4. ✅ **実行して確認**

### 完了後

- シーンを跨いでも自動生成は継続
- タイマー一時停止時は自動で生成停止
- タイマー終了時は統計が自動表示
- GameDatabase にお金・アイテムが自動追加

---

## 関連ドキュメント

詳細な情報は以下を参照：

- **FACILITY_PRODUCTION_USAGE_GUIDE.md** - 使い方の完全ガイド
- **README_FacilityProduction.md** - 技術仕様書
- **TIMER_INTEGRATION_GUIDE.md** - TimerManager統合詳細
- **INDEX.md** - ドキュメント索引

---

## FAQ

### Q: 複数の設備を同時に稼働できますか？
はい、RegisterFacility で登録した設備はすべて同時に稼働します。

### Q: 生成間隔は後から変更できますか？
はい、facility.SetProductionInterval(新しい秒数) で変更可能です。

### Q: TimerManager なしでも動きますか？
はい、Time.time でフォールバック動作します。

### Q: 一時停止できますか？
はい、SetFacilityProductionPaused(facilityId, true) で可能です。

---

**以上で設定完了です！🎉**
