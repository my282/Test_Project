# Hub Page システムの使い方

## 概要
色々なページ（シーン）に遷移できるハブページシステムを作成しました。ボタンの追加やデザインの変更がインスペクターから簡単にできます。

## 作成したスクリプト

### 1. PageButtonData.cs
各ボタンの設定を保持するデータクラス
- シーン名
- ボタンテキスト
- 背景色、テキスト色
- フォントサイズ
- ボタンのサイズ（幅・高さ）

### 2. HubPageManager.cs
ハブページを管理するメインスクリプト
- ボタンの自動生成
- レイアウト設定（縦・横配置）
- ボタン間のスペース調整
- カスタムボタンプレハブのサポート

### 3. SceneLoader.cs
シーン遷移をサポートするユーティリティ
- フェードエフェクト付き遷移（オプション）
- ハブページへの戻るボタン
- 前後のシーンへの移動

## セットアップ手順

### 1. hub_page シーンの設定
1. `hub_page.unity` シーンを開く
2. Canvas を作成（なければ）
3. 空のGameObjectを作成し、`HubPageManager` という名前にする
4. HubPageManager コンポーネントをアタッチ

### 2. ボタンの追加方法
HubPageManager のインスペクターで：
1. `Page Buttons` の `+` ボタンをクリック
2. 各項目を設定：
   - **Scene Name**: 遷移先のシーン名（例: "SampleScene"）
   - **Button Text**: ボタンに表示する文字（例: "サンプルへ"）
   - **Background Color**: ボタンの背景色
   - **Text Color**: 文字の色
   - **Font Size**: 文字サイズ（10-72）
   - **Button Width**: ボタンの幅（100-600）
   - **Button Height**: ボタンの高さ（30-200）

### 3. レイアウト設定
- **Button Spacing**: ボタン間の間隔
- **Vertical Layout**: チェックで縦配置、未チェックで横配置

### 4. ビルド設定
遷移先のシーンを Build Settings に追加：
1. `File > Build Settings`
2. 使用するシーンをドラッグ＆ドロップ

## 使用例

```
Page Buttons リスト:
1. Scene Name: "SampleScene", Button Text: "サンプル", 背景色: 青
2. Scene Name: "GameScene", Button Text: "ゲーム開始", 背景色: 緑
3. Scene Name: "SettingsScene", Button Text: "設定", 背景色: グレー
```

## 追加機能
- SceneLoader を使用して、他のシーンからハブページに戻ることができます
- `SceneLoader.ReturnToHub()` を呼び出すだけ

## カスタマイズ
- より複雑なデザインが必要な場合は、カスタムボタンプレハブを作成して `Custom Button Prefab` に設定できます
