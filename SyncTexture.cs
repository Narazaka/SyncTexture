using JetBrains.Annotations;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTexture : UdonSharpBehaviour
    {
        [SerializeField]
        public Texture2D Source;
        [SerializeField]
        public Texture2D Target;
        [SerializeField]
        public SendFormat SendFormat;
        [SerializeField]
        public int GetPixelsBulkCount = 8;
        [SerializeField]
        public int BulkCount = 5000;
        [SerializeField]
        public float SyncInterval = 1f;
        [SerializeField]
        public bool ShowProgress = true;
        [SerializeField]
        UdonBehaviour CallbackListener;

        [UdonSynced]
        ushort[] SyncColors;
        [UdonSynced]
        short SyncIndex = -1;

        int ReadIndex = -1;
        ushort[] Colors = new ushort[0];
        ushort[] ReceiveColors = new ushort[0];

        [PublicAPI]
        public bool CanStartSync { get => SyncIndex == -1 && ReadIndex == -1; }

        [PublicAPI]
        public float Progress
        {
            get
            {
                if (SyncIndex == -1) return 0f;
                var packUnitLength = ColorEncoder.PackUnitLength(SendFormat);
                var dataLen = Target.width * Target.height * packUnitLength;
                return (float)SyncIndex * BulkCount / dataLen;
            }
        }

        /// <summary>
        /// Take ownership and send texture data to other players.
        /// </summary>
        [PublicAPI]
        public bool StartSync()
        {
            if (!CanStartSync) return false;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Callback(nameof(SyncTextureCallbackListener.OnPreSync));
            PrepareSync();
            return true;
        }

        [PublicAPI]
        public bool ForceStartSync()
        {
            ReadIndex = -1;
            SyncIndex = -1;
            return StartSync();
        }

        public void PrepareSync()
        {
            var size = Source.width * Source.height;
            Colors = new ushort[ColorEncoder.PackUnitLength(SendFormat) * size];
            ReadIndex = -1;
            ReadPixels();
        }

        public void ReadPixels()
        {
            ReadIndex++;
            var startHeight = ReadIndex * GetPixelsBulkCount;
            Debug.Log($"[SyncTexture] ReadPixels from height={startHeight}");
            if (startHeight >= Source.height)
            {
                Callback(nameof(SyncTextureCallbackListener.OnSyncStart));
                ReadIndex = -1;
                SyncIndex = -1;
                SyncNext();
                return;
            }
            var height = Mathf.Min(GetPixelsBulkCount, Source.height - startHeight);
            var colors = Source.GetPixels(0, startHeight, Source.width, height);
            ColorEncoder.Pack(colors, 0, Colors, startHeight * Source.width, colors.Length, SendFormat);
            SendCustomEventDelayedFrames(nameof(ReadPixels), 1);
        }

        public void SyncNext()
        {
            ++SyncIndex;
            var len = Colors.Length;
            var startIndex = SyncIndex * BulkCount;
            var count = Mathf.Min(BulkCount, len - startIndex);
            if (count <= 0)
            {
                SyncIndex = -1;
                SyncColors = new ushort[0];
                QueueSerialization();
                return;
            }
            Debug.Log($"[SyncTexture] SyncNext from height={startIndex}/{len}");
            SyncColors = new ushort[count];
            Array.Copy(Colors, startIndex, SyncColors, 0, count);
            QueueSerialization();
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (SyncIndex == -1)
            {
                Debug.Log($"[SyncTexture] Sent");
                Callback(nameof(SyncTextureCallbackListener.OnSyncComplete));
                return;
            }
            if (SyncIndex >= 0)
            {
                Callback(nameof(SyncTextureCallbackListener.OnSync));
                SendCustomEventDelayedSeconds(nameof(SyncNext), SyncInterval);
            }
        }

        public override void OnDeserialization()
        {
            if (SyncIndex == -1)
            {
                if (ReceiveColors == null || ReceiveColors.Length == 0) return;
                if (!ShowProgress)
                {
                    Target.SetPixels(ColorEncoder.Unpack(ReceiveColors, SendFormat));
                    Target.Apply();
                }
                Debug.Log($"[SyncTexture] Received");
                Callback(nameof(SyncTextureCallbackListener.OnReceiveComplete));
                return;
            }

            var packUnitLength = ColorEncoder.PackUnitLength(SendFormat);
            var receiveDataLen = Target.width * Target.height * packUnitLength;
            if (SyncIndex == 0 || ReceiveColors == null || ReceiveColors.Length != receiveDataLen)
            {
                ReceiveColors = new ushort[receiveDataLen];
                Callback(nameof(SyncTextureCallbackListener.OnReceiveStart));
            }
            Array.Copy(SyncColors, 0, ReceiveColors, SyncIndex * BulkCount, SyncColors.Length);
            if (ShowProgress)
            {
                var minHeight = SyncIndex * BulkCount / Target.width / packUnitLength;
                var maxHeight = Mathf.Min((SyncIndex + 1) * BulkCount / Target.width / packUnitLength, Target.height);
                var height = maxHeight - minHeight;
                var partColors = new ushort[height * Target.width * packUnitLength];
                if (partColors.Length == 0) return;
                Debug.Log($"[SyncTexture] Deserialized {minHeight}->{maxHeight}({height}) datalen={partColors.Length}");
                Array.Copy(ReceiveColors, minHeight * Target.width * packUnitLength, partColors, 0, partColors.Length);
                var colors = ColorEncoder.Unpack(partColors, SendFormat);
                Target.SetPixels(0, minHeight, Target.width, height, colors);
                Target.Apply();
            }
            Callback(nameof(SyncTextureCallbackListener.OnReceive));
        }

        void Callback(string eventName)
        {
            if (CallbackListener != null)
            {
                CallbackListener.SendCustomEvent(eventName);
            }
        }

        void QueueSerialization()
        {
            RequestSerialization();
#if UNITY_EDITOR
            OnPostSerialization(default);
            OnDeserialization();
#endif
        }
    }
}
