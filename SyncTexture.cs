using JetBrains.Annotations;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTexture : SyncTextureBase
    {
        [SerializeField]
        public UdonBehaviour PrepareCallbackListener;
        [SerializeField]
        public int BulkCount = 5000;
        [SerializeField]
        public float SyncInterval = 1f;
        [SerializeField]
        public bool ShowProgress = true;
        [SerializeField]
        bool PrepareCallbackAsync;

        [UdonSynced]
        short SyncIndex = -1;

        bool Prepareing;

        [PublicAPI]
        public bool CanStartSync { get => SyncIndex < 0 && !ReadingSource && !Prepareing; }

        [PublicAPI]
        public float Progress
        {
            get
            {
                if (SyncIndex < 0) return 0f;
                var dataLen = Width * Height * PackUnitLength;
                return (float)SyncIndex * BulkCount / dataLen;
            }
        }

        abstract protected int Width { get; }
        abstract protected int Height { get; }
        abstract protected bool ReadingSource { get; }
        abstract protected void StartReadSource();
        abstract protected void CancelReadSource();
        abstract protected void ApplyReceiveColors();
        abstract protected void ApplyReceiveColorsPartial(int minHeight, int height);

        abstract protected int PackUnitLength { get; }

        abstract protected void InitializeSyncColors(int pixelLength);
        abstract protected void InitializeSourceColors();
        abstract protected void InitializeReceiveColors();
        abstract protected int SourceColorsLength { get; }
        abstract protected bool ReceiveColorsIsEmpty { get; }
        abstract protected bool ReceiveColorsIsValid { get; }
        abstract protected void CopySourceColorsToSyncColors(int startSourceIndex, int pixelLength);
        abstract protected void CopySyncColorsToReceiveColors(int startReceiveIndex);

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
            CancelReadSource();
            SyncIndex = -2;
            if (Prepareing)
            {
                PrepareCallback(nameof(SyncTexturePrepareCallbackListener.OnPrepareCancel));
            }
            Prepareing = false;
            InitializeSyncColors(0);
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
            InitializeSourceColors();
            StartReadSource();
        }

        protected void StartSyncNext()
        {
            Callback(nameof(SyncTextureCallbackListener.OnSyncStart));
            SyncIndex = -1;
            SyncNext();
        }

        public void SyncNext()
        {
            if (SyncIndex == -2)
            {
                return;
            }
            ++SyncIndex;
            var len = SourceColorsLength;
            var bulkPixelCount = BulkCount / PackUnitLength;
            var startIndex = SyncIndex * bulkPixelCount;
            var count = Mathf.Min(bulkPixelCount, len - startIndex);
            if (count <= 0)
            {
                SyncIndex = -1;
                InitializeSyncColors(0);
                QueueSerialization();
                return;
            }
            Debug.Log($"[SyncTexture] SyncNext from height={startIndex}/{len}");
            InitializeSyncColors(count);
            CopySourceColorsToSyncColors(startIndex, count);
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
                if (ReceiveColorsIsEmpty) return;
                if (!ShowProgress)
                {
                    ApplyReceiveColors();
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
            if (SyncIndex == 0 || !ReceiveColorsIsValid)
            {
                InitializeReceiveColors();
                Callback(nameof(SyncTextureCallbackListener.OnReceiveStart));
            }
            CopySyncColorsToReceiveColors(SyncIndex * BulkCount);
            if (ShowProgress)
            {
                var minHeight = SyncIndex * BulkCount / Width / packUnitLength;
                var maxHeight = Mathf.Min((SyncIndex + 1) * BulkCount / Width / packUnitLength, Height);
                var height = maxHeight - minHeight;
                if (height == 0) return;
                Debug.Log($"[SyncTexture] Deserialized {minHeight}->{maxHeight}({height}) datalen={height * Width * packUnitLength}");
                ApplyReceiveColorsPartial(minHeight, height);
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
