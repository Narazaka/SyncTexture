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

```
// SyncTextureManager
bool Sending;
void RequestSyncTextureByIndex(int index, bool resendWhenExistsAndNowSending = true);
void RequestSyncTexture(SyncTextureBase syncTexture, bool resendWhenExistsAndNowSending = true);
```

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
