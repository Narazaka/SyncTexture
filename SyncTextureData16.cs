using System;
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    [AddComponentMenu("Sync Texture/Sync Texture Data/Sync Texture Data16")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTextureData16 : SyncTextureData
    {
        [SerializeField]
        SyncTexture2D16 SyncTexture2D;
        [UdonSynced, NonSerialized]
        public ushort[] Data;

        public void Send(ushort[] data)
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
