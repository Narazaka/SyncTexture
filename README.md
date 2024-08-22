# Sync Texture

Sync Texture2D

## About

Texture2Dを同期します。Render TextureからTexture2Dへの変換などは含みません。

## Install

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「SyncTexture」をインストールします。

## 使い方

「SyncTexture」コンポーネントをオブジェクトに追加し、設定します。

基本的に他のUdonからの制御を前提にしています。

- SyncTextureManager: 順番に同期
- SyncTexturesOnLateJoin: late joinerが来たら順番に同期を開始

```
// SyncTexture
bool CanStartSync;
float Progress;
void StartSync(); // take ownership and send
void ForceStartSync();

// SyncTextureManager
bool Sending;
void RequestResend();
void StartSyncAll(bool requestResendWhenSending = false);
void ForceStartSyncAll();
void CancelSync();
```

## 更新履歴

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
