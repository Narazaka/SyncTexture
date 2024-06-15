
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
        public SyncTextureBase[] SyncTextures;
        [UdonSynced]
        sbyte SendingIndex = -1;
        [UdonSynced]
        bool Resend;

        [PublicAPI]
        public bool Sending => SendingIndex >= 0;

        [PublicAPI]
        public void RequestResend()
        {
            Resend = true;
            RequestSerialization();
        }

        [PublicAPI]
        public void StartSyncAll(bool requestResendWhenSending = false)
        {
            if (Sending)
            {
                if (requestResendWhenSending)
                {
                    RequestResend();
                }
                return;
            }
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
            if (!Sending) return;
            SyncTextures[SendingIndex].CancelSync();
            SendingIndex = -1;
            RequestSerialization();
        }

        public void SendNext()
        {
            do
            {
                SendingIndex++;
                if (SendingIndex >= SyncTextures.Length)
                {
                    SendingIndex = -1;
                    if (Resend)
                    {
                        Resend = false;
                        StartSyncAll();
                    }
                    RequestSerialization();
                    return;
                }
            }
            while (!SyncTextures[SendingIndex].SyncEnabled);
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
            if (!Sending) return;
            SyncTextures[SendingIndex].ForceStartSync();
        }
    }
}
