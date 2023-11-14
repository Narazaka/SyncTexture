
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace net.narazaka.vrchat.sync_texture
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTextureManager : UdonSharpBehaviour
    {
        [SerializeField]
        SyncTexture[] SyncTextures;
        [UdonSynced]
        sbyte SendingIndex = -1;

        [PublicAPI]
        public void StartSyncAll()
        {
            if (SendingIndex >= 0) return;
            ForceStartSyncAll();
        }

        [PublicAPI]
        public void ForceStartSyncAll()
        {
            CancelSync();
            SendingIndex = -1;
            RequestSerialization();
            SendNext();
        }

        [PublicAPI]
        public void CancelSync()
        {
            if (SendingIndex < 0) return;
            SyncTextures[SendingIndex].CancelSync();
            SendingIndex = -1;
            RequestSerialization();
        }

        public void SendNext()
        {
            SendingIndex++;
            if (SendingIndex >= SyncTextures.Length)
            {
                SendingIndex = -1;
                RequestSerialization();
                return;
            }
            while (!SyncTextures[SendingIndex].SyncEnabled)
            {
                SendingIndex++;
            }
            RequestSerialization();
            SyncTextures[SendingIndex].StartSync();
        }

        public void OnSyncComplete()
        {
            SendNext();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (SendingIndex < 0) return;
            SyncTextures[SendingIndex].ForceStartSync();
        }
    }
}
