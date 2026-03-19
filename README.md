# SyncTexture

VRChatワールドでTexture2Dをネットワーク同期するためのUdonSharpライブラリです。

## 特徴

- Texture2Dのピクセルデータをプレイヤー間でネットワーク同期
- Late joiner（後からインスタンスに入ったプレイヤー）への高速同期
- 複数のカラーエンコーダーに対応（用途に応じて帯域と画質のバランスを選択可能）
- VRCAsyncGPUReadbackによる高速テクスチャ読み取り対応
- キューベースの順次送信管理（SyncTextureManager）

## 動作要件

- Unity 2022.3以上
- VRChat Worlds SDK 3.2.0以上

## インストール

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. ワールドプロジェクトの「Manage Project」から「SyncTexture」をインストールします。

## 使い方

### 基本セットアップ

1. 同期したいテクスチャを持つGameObjectに `SyncTexture2D8`（8bit）または `SyncTexture2D16`（16bit）コンポーネントを追加します。
2. 同じGameObjectまたは別のGameObjectに `SyncTextureManager` コンポーネントを追加し、管理対象のSyncTextureを登録します。
3. 用途に合ったColorEncoderコンポーネントを追加し、SyncTexture2Dに設定します。

### カラーエンコーダーの選択

用途に応じて以下のエンコーダーから選択してください。

| エンコーダー | データ量 | 用途 |
|---|---|---|
| `ColorEncoderR8` | 1バイト/ピクセル | グレースケール（赤チャンネルのみ） |
| `ColorEncoderRG88` | 2バイト/ピクセル | 2チャンネル（赤・緑） |
| `ColorEncoderRGB565` | 2バイト/ピクセル | カラー（やや低品質、帯域節約） |
| `ColorEncoderRGB888` | 3バイト/ピクセル | カラー（高品質） |
| `ColorEncoderRGBA4444` | 2バイト/ピクセル | 半透明対応（低品質） |
| `ColorEncoderRGBA8888` | 4バイト/ピクセル | 半透明対応（高品質） |

データ量が小さいほど同期速度が速くなります。必要十分なエンコーダーを選ぶことで、同期時間を短縮できます。

### コンポーネントの設定項目

#### SyncTexture2D

| プロパティ | 説明 |
|---|---|
| `Source` | 読み取り元のTexture2D |
| `Target` | 書き込み先のTexture2D |
| `Width` / `Height` | テクスチャのサイズ |
| `BulkLineCount` | 1回の送信で同期するライン数。`0` = 1秒あたりの帯域上限から自動計算、`-1` = 1回のシリアライゼーション上限から自動計算 |
| `BulkRateOfNetworkSpec` | ネットワーク帯域の使用率（デフォルト: `0.8` = 80%） |
| `SyncInterval` | チャンク間の送信間隔（秒、デフォルト: `1`） |
| `GetPixelsBulkCount` | 1フレームあたりのピクセル読み取り数。`0` = VRCAsyncGPUReadbackを使用 |
| `ReceiveEnabled` | 受信処理の有効/無効 |

#### SyncTextureManager

| プロパティ | 説明 |
|---|---|
| `SyncTextures` | 管理対象のSyncTextureコンポーネントの配列 |

### API

基本的に他のUdonからの制御を前提にしています。

#### SyncTextureManager

```csharp
// 送信中かどうか
bool Sending;

// インデックス指定で同期をリクエスト
void RequestSyncTextureByIndex(int index, bool resendWhenExistsAndNowSending = true);

// SyncTexture参照で同期をリクエスト
void RequestSyncTexture(SyncTextureBase syncTexture, bool resendWhenExistsAndNowSending = true);
```

#### SyncTexture

```csharp
// 同期を開始できるか
bool CanStartSync;

// 同期の進捗（0.0〜1.0）
float Progress;

// 同期を開始（オーナーシップを取得して送信）
bool StartSync();

// 同期を強制開始（実行中の同期をキャンセルして再開始）
bool ForceStartSync();

// 同期をキャンセル
bool CancelSync();
```

### 使用例

```csharp
// SyncTextureManagerを使ってテクスチャ同期をリクエスト
syncTextureManager.RequestSyncTexture(syncTexture);
```

インタラクトで同期を開始するサンプルが `Samples/` フォルダに含まれています。

### コールバック

`SyncTextureCallbackListener` を継承したUdonSharpBehaviourを作成し、SyncTextureの `CallbackListeners` に登録すると、同期のライフサイクルイベントを受け取れます。

| コールバック | タイミング |
|---|---|
| `OnPreSync` | 同期処理の開始前 |
| `OnPrepare` | 準備開始時 |
| `OnPrepareCancel` | 準備がキャンセルされた時 |
| `OnSyncStart` | データ送信の開始時 |
| `OnSync` | 各チャンクの送信完了時 |
| `OnSyncComplete` | 全データの送信完了時 |
| `OnSyncCanceled` | 同期がキャンセルされた時 |
| `OnReceive` | データの受信時 |
| `OnReceiveApplied` | 受信データのテクスチャへの適用時 |

## ネットワーク仕様について

VRChatのUdonネットワーク仕様に基づき、以下の制約があります。

- 1回のシリアライゼーションあたり最大約64,690バイト
- 1秒あたり最大約11KB

`BulkLineCount` を `0`（デフォルト）にすると、1秒あたりの帯域上限を基準にチャンクサイズを自動計算します。大きなテクスチャを高速に同期したい場合は `-1` に設定してください（ただし帯域を多く消費します）。

## 更新履歴

- 3.0.0
  - アーキテクチャの一新
    - インスタンスに情報が保持される仕組みにし、一般的なケースでlate joinerの同期速度を飛躍的に向上。
  - 破壊的変更
    - アーキテクチャの一新によりAPIが大幅に変更されました。
    - 一般的なケースでは以下の手順でマイグレーション出来ると思います。
      1. SyncTexture2DのBulkLineCountを再設定する。
      2. 任意のSyncTexture2Dの「Set All DataList」ボタンを押す。
      3. SyncTexturesOnLateJoinを削除し、`SyncTextureManager.RequestSyncTexture()`を呼ぶ仕組みを実装する。
- 2.0.0
  - 新機能
    - VRCAsyncGPUReadbackを用いた高速読取処理が可能に
    - 再送信の考慮
    - 新しいColorEncoder
  - 破壊的変更
    - ColorEncoder指定まわりが変更され、再設定が必要になっています。
    - コールバックAPIのCallbackListener, PrepareCallbackListenerがCallbackListenersに統合されています。
- 1.3.0
  - サンプル追加
- 1.2.0
  - 同期の前に呼ばれるOnPrepare/OnPrepared APIを追加
- 1.1.0
  - add: SyncTextureManager / SyncTexturesOnLateJoin
- 1.0.0
  - リリース

## License

[Zlib License](LICENSE.txt)
