
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace net.narazaka.vrchat.sync_texture
{
    [AddComponentMenu("Sync Texture/Sync Texture Manager")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTextureManager : UdonSharpBehaviour
    {
        [SerializeField]
        public SyncTextureBase[] SyncTextures;
        [UdonSynced]
        sbyte[] SendIndexQueue = new sbyte[0];

        [PublicAPI]
        public bool Sending => SendIndexQueue.Length > 0;

        sbyte SendingIndex => SendIndexQueue.Length > 0 ? SendIndexQueue[0] : (sbyte)-1;

        /// <summary>
        /// Start sending textures.
        /// 
        /// If you are not the owner of the object, do nothing.
        /// </summary>
        /// <param name="index">the index of SyncTexture</param>
        /// <param name="resendWhenExistsAndNowSending">resend when exists and now sending</param>
        [PublicAPI]
        public void RequestSyncTextureByIndex(int index, bool resendWhenExistsAndNowSending = true)
        {
            if (!Networking.IsOwner(gameObject)) return;

            var existIndex = System.Array.IndexOf(SendIndexQueue, (sbyte)index);
            Debug.Log($"[SyncTextureManager] RequestSyncTextureByIndex({index}) exist queue index={existIndex}");
            if (existIndex != -1)
            {
                if (existIndex == 0 && resendWhenExistsAndNowSending)
                {
                    Send();
                }
                return;
            }
            PushQueue((sbyte)index);
            RequestSerialization();

            if (SendIndexQueue.Length == 1)
            {
                Send();
            }
        }

        /// <summary>
        /// Start sending textures.
        /// 
        /// If you are not the owner of the object, do nothing.
        /// </summary>
        /// <param name="syncTexture">the SyncTexture you want</param>
        /// <param name="resendWhenExistsAndNowSending">resend when exists and now sending</param>
        [PublicAPI]
        public void RequestSyncTexture(SyncTextureBase syncTexture, bool resendWhenExistsAndNowSending = true)
        {
            RequestSyncTextureByIndex(System.Array.IndexOf(SyncTextures, syncTexture), resendWhenExistsAndNowSending);
        }

        void PushQueue(sbyte index)
        {
            var newToSendIndexes = new sbyte[SendIndexQueue.Length + 1];
            SendIndexQueue.CopyTo(newToSendIndexes, 0);
            newToSendIndexes[SendIndexQueue.Length] = index;
            SendIndexQueue = newToSendIndexes;
        }

        void ShiftQueue()
        {
            if (SendIndexQueue.Length == 0) return;
            var newToSendIndexes = new sbyte[SendIndexQueue.Length - 1];
            System.Array.Copy(SendIndexQueue, 1, newToSendIndexes, 0, newToSendIndexes.Length);
            SendIndexQueue = newToSendIndexes;
        }

        void Send()
        {
            if (!Sending) return;

            SyncTextures[SendingIndex].ForceStartSync();
        }

        public void OnSyncComplete()
        {
            ShiftQueue();
            RequestSerialization();

            Send();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            
            Send();
        }
    }
}
