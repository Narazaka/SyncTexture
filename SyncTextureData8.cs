using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    [AddComponentMenu("Sync Texture/Sync Texture Data/Sync Texture Data8")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTextureData8 : SyncTextureData
    {
        [SerializeField]
        SyncTexture2D8 SyncTexture2D;
        [UdonSynced, NonSerialized]
        public byte[] Data;

        public void Send(byte[] data)
        {
            Data = data;
            RequestSerialization();
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            SyncTexture2D.OnOneSyncDone(result.success);
        }

        protected override bool DataIsNull => Data == null;
        protected override bool DataIsEmpty => Data.Length == 0;
        protected override void ApplyReceiveData()
        {
            SyncTexture2D.ApplyReceiveColorsPartial(this);
        }
    }
}
