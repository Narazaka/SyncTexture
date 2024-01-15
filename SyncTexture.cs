using JetBrains.Annotations;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTexture<T> : SyncTextureBase where T : struct
    {
        [SerializeField]
        public UdonBehaviour PrepareCallbackListener;
        [SerializeField]
        public Texture2D Source;
        [SerializeField]
        public Texture2D Target;
        [SerializeField]
        public int GetPixelsBulkCount = 8;
        [SerializeField]
        public int BulkCount = 5000;
        [SerializeField]
        public float SyncInterval = 1f;
        [SerializeField]
        public bool ShowProgress = true;
        [SerializeField]
        bool PrepareCallbackAsync;

        [UdonSynced]
        T[] SyncColors;
        [UdonSynced]
        short SyncIndex = -1;

        int ReadIndex = -1;
        T[] Colors = new T[0];
        T[] ReceiveColors = new T[0];

        bool Prepareing;

        [PublicAPI]
        public bool CanStartSync { get => SyncIndex < 0 && ReadIndex < 0 && !Prepareing; }

        [PublicAPI]
        public float Progress
        {
            get
            {
                if (SyncIndex < 0) return 0f;
                var dataLen = Target.width * Target.height * PackUnitLength;
                return (float)SyncIndex * BulkCount / dataLen;
            }
        }

        abstract protected int PackUnitLength { get; }
        abstract protected void PackColors(Color[] colors, int startColorIndex, T[] data, int startPixelIndex, int pixelLength);
        abstract protected Color[] UnpackColors(T[] colors);

        /// <summary>
        /// Take ownership and send texture data to other players.
        /// </summary>
        [PublicAPI]
        public override bool StartSync()
        {
            if (!CanStartSync || !SyncEnabled) return false;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Callback(nameof(SyncTextureCallbackListener.OnPreSync));
            if (PrepareCallbackAsync)
            {
                Prepareing = true;
            }
            PrepareCallback(nameof(SyncTexturePrepareCallbackListener.OnPrepare));
            if (!Prepareing)
            {
                PrepareSync();
            }
            return true;
        }

        [PublicAPI]
        public override bool ForceStartSync()
        {
            CancelSync();
            return StartSync();
        }

        [PublicAPI]
        public override bool CancelSync()
        {
            if (CanStartSync) return false;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            ReadIndex = -2;
            SyncIndex = -2;
            if (Prepareing)
            {
                PrepareCallback(nameof(SyncTexturePrepareCallbackListener.OnPrepareCancel));
            }
            Prepareing = false;
            SyncColors = new T[0];
            QueueSerialization();
            Debug.Log($"[SyncTexture] Send Canceled");
            Callback(nameof(SyncTextureCallbackListener.OnSyncCanceled));
            return true;
        }


        /// <summary>
        /// called by <see cref="PrepareCallbackListener"/>
        /// </summary>
        [PublicAPI]
        public void OnPrepared()
        {
            if (Prepareing)
            {
                Prepareing = false;
                PrepareSync();
            }
        }

        public void PrepareSync()
        {
            var size = Source.width * Source.height;
            Colors = new T[PackUnitLength * size];
            ReadIndex = -1;
            ReadPixels();
        }

        public void ReadPixels()
        {
            if (ReadIndex == -2)
            {
                return;
            }
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
            PackColors(colors, 0, Colors, startHeight * Source.width, colors.Length);
            SendCustomEventDelayedFrames(nameof(ReadPixels), 1);
        }

        public void SyncNext()
        {
            if (SyncIndex == -2)
            {
                return;
            }
            ++SyncIndex;
            var len = Colors.Length;
            var startIndex = SyncIndex * BulkCount;
            var count = Mathf.Min(BulkCount, len - startIndex);
            if (count <= 0)
            {
                SyncIndex = -1;
                SyncColors = new T[0];
                QueueSerialization();
                return;
            }
            Debug.Log($"[SyncTexture] SyncNext from height={startIndex}/{len}");
            SyncColors = new T[count];
            Array.Copy(Colors, startIndex, SyncColors, 0, count);
            QueueSerialization();
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (SyncIndex == -2)
            {
                return;
            }
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
            if (SyncIndex < 0)
            {
                if (ReceiveColors == null || ReceiveColors.Length == 0) return;
                if (!ShowProgress)
                {
                    Target.SetPixels(UnpackColors(ReceiveColors));
                    Target.Apply();
                }
                if (SyncIndex == -2)
                {
                    Debug.Log($"[SyncTexture] Receive Canceled");
                    Callback(nameof(SyncTextureCallbackListener.OnReceiveCanceled));
                    return;
                }
                Debug.Log($"[SyncTexture] Received");
                Callback(nameof(SyncTextureCallbackListener.OnReceiveComplete));
                return;
            }

            var packUnitLength = PackUnitLength;
            var receiveDataLen = Target.width * Target.height * packUnitLength;
            if (SyncIndex == 0 || ReceiveColors == null || ReceiveColors.Length != receiveDataLen)
            {
                ReceiveColors = new T[receiveDataLen];
                Callback(nameof(SyncTextureCallbackListener.OnReceiveStart));
            }
            Array.Copy(SyncColors, 0, ReceiveColors, SyncIndex * BulkCount, SyncColors.Length);
            if (ShowProgress)
            {
                var minHeight = SyncIndex * BulkCount / Target.width / packUnitLength;
                var maxHeight = Mathf.Min((SyncIndex + 1) * BulkCount / Target.width / packUnitLength, Target.height);
                var height = maxHeight - minHeight;
                var partColors = new T[height * Target.width * packUnitLength];
                if (partColors.Length == 0) return;
                Debug.Log($"[SyncTexture] Deserialized {minHeight}->{maxHeight}({height}) datalen={partColors.Length}");
                Array.Copy(ReceiveColors, minHeight * Target.width * packUnitLength, partColors, 0, partColors.Length);
                var colors = UnpackColors(partColors);
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

        void PrepareCallback(string eventName)
        {
            if (PrepareCallbackListener != null)
            {
                PrepareCallbackListener.SendCustomEvent(eventName);
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
