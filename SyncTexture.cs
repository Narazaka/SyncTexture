using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTexture : SyncTextureBase
    {
        // cf. https://creators.vrchat.com/worlds/udon/networking/network-details
        /// <summary>
        /// Udon scripts with manual sync are limited to roughly 64690 bytes per serialization.
        /// </summary>
        public const int MaxBulkBytesPerSerialization = 64690;
        /// <summary>
        /// Udon scripts can send out about 11 kilobytes per second.
        /// </summary>
        public const int MaxBulkBytesPerSecond = 11264; // 11 * 1024;

        /// <summary>
        /// bulk send line count.
        /// 
        /// 0: auto calculate by network spec per second
        /// -1: auto calculate by network spec per serialization
        /// </summary>
        [SerializeField]
        public int BulkLineCount = 0;
        /// <summary>
        /// rate of network spec.
        /// 
        /// used when BulkLineCount is 0 or -1
        /// </summary>
        [SerializeField]
        public float BulkRateOfNetworkSpec = 0.8f;
        [SerializeField]
        public float SyncInterval = 1f;
        [SerializeField]
        public bool PrepareCallbackAsync;

        protected short SyncIndex = -1; // line index

        bool Prepareing;

        [PublicAPI]
        public bool CanStartSync { get => SyncIndex < 0 && !ReadingSource && !Prepareing; }

        [PublicAPI]
        public float Progress
        {
            get
            {
                if (SyncIndex < 0) return 0f;
                return (float)SyncIndex * BulkUnitCount / AllUnitCount;
            }
        }

        [PublicAPI]
        public float DataLimitRatePerSerialization => (float)BulkByteCount / MaxBulkBytesPerSerialization;

        [PublicAPI]
        public float DataLimitRatePerSecond => (float)BulkByteCount / MaxBulkBytesPerSecond;

        [PublicAPI]
        public static int GetChunkCount(int height, int effectiveBulkLineCount) => Mathf.CeilToInt((float) height / effectiveBulkLineCount);

        [PublicAPI]
        public static int GetEffectiveBulkLineCount(int bulkLineCount, float bulkRateOfNetworkSpec, int width, int unitByteLength, int packUnitLength) =>
            bulkLineCount == 0
            ? (int)(MaxBulkBytesPerSecond * bulkRateOfNetworkSpec) / (width * packUnitLength * unitByteLength)
            : bulkLineCount == -1
            ? (int)(MaxBulkBytesPerSerialization * bulkRateOfNetworkSpec) / (width * packUnitLength * unitByteLength)
            : bulkLineCount;
        [PublicAPI]
        public int EffectiveBulkLineCount => GetEffectiveBulkLineCount(BulkLineCount, BulkRateOfNetworkSpec, Width, UnitByteLength, PackUnitLength);

        [PublicAPI]
        int BulkPixelCount => EffectiveBulkLineCount * Width;
        [PublicAPI]
        int BulkUnitCount => BulkPixelCount * PackUnitLength;
        [PublicAPI]
        int BulkByteCount => BulkUnitCount * UnitByteLength;

        int AllUnitCount => Width * Height * PackUnitLength;

        [PublicAPI]
        abstract public int UnitByteLength { get; }
        abstract protected int PackUnitLength { get; }

        abstract protected int Width { get; }
        abstract protected int Height { get; }
        abstract protected bool ReadingSource { get; }
        abstract protected void StartReadSource();
        abstract protected void CancelReadSource();

        abstract protected int SourceColorsLength { get; }
        abstract protected void SyncColors(int startSourceIndex, int pixelLength);

        /// <summary>
        /// Take ownership and send texture data to other players.
        /// </summary>
        [PublicAPI]
        public override bool StartSync()
        {
            if (!CanStartSync) return false;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Callback(nameof(SyncTextureCallbackListener.OnPreSync));
            if (PrepareCallbackAsync)
            {
                Prepareing = true;
            }
            Callback(nameof(SyncTextureCallbackListener.OnPrepare));
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
                Callback(nameof(SyncTextureCallbackListener.OnPrepareCancel));
            }
            Prepareing = false;
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
            var startIndex = SyncIndex * BulkPixelCount;
            var count = Mathf.Min(BulkPixelCount, len - startIndex);
            if (count <= 0)
            {
                SyncIndex = -1;
                QueueSerialization();
                Debug.Log($"[SyncTexture] Sent");
                Callback(nameof(SyncTextureCallbackListener.OnSyncComplete));
                return;
            }
            Debug.Log($"[SyncTexture] SyncNext from height={startIndex}/{len}");
            SyncColors(startIndex, count);
            QueueSerialization();
        }

        // called by one data callback
        public void OnOneSyncDone(bool success)
        {
            if (SyncIndex == -2)
            {
                return;
            }
            Callback(nameof(SyncTextureCallbackListener.OnSync));
            SendCustomEventDelayedSeconds(nameof(SyncNext), SyncInterval);
        }

        protected void Callback(string eventName)
        {
            if (CallbackListeners != null)
            {
                foreach (var listener in CallbackListeners)
                {
                    listener.SendCustomEvent(eventName);
                }
            }
        }

        void QueueSerialization()
        {
        }
    }
}
