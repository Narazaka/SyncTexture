using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTextureData8 : SyncTextureData
    {
        [SerializeField]
        SyncTexture2D8 SyncTexture2D;
        [UdonSynced, NonSerialized]
        public byte[] Data = new byte[0];

        public void Send(byte[] data)
        {
            Data = data;
            RequestSerialization();
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            SyncTexture2D.OnOneSyncDone(result.success);
        }

        public override void OnDeserialization()
        {
            if (Data == null || Data.Length == 0) return;

            SyncTexture2D.ApplyReceiveColorsPartial(this);
        }
    }
}
