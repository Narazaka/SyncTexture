
using UdonSharp;
using UnityEngine;
using System;
using VRC.Udon;
using VRC.SDK3.Rendering;
using VRC.Udon.Common.Interfaces;


namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTexture2D : SyncTexture
    {
        [SerializeField]
        public Texture Source;
        [SerializeField]
        public Texture2D Target;
        [SerializeField]
        public int GetPixelsBulkCount = 8;

        int ReadIndex = -1;

        protected Color32[] SourceColors = new Color32[0];

        void StoreSourceColors(Color32[] colors, int startPixelIndex) => Array.Copy(colors, 0, SourceColors, startPixelIndex, colors.Length);
        abstract protected Color[] UnpackReceiveColors();
        abstract protected Color[] UnpackReceiveColorsPartial(int startReceivePixelIndex, int pixelLength);

        protected override bool ReadingSource => ReadIndex >= 0;
        protected override int Width => Source.width;
        protected override int Height => Source.height;
        protected override void InitializeSourceColors() => SourceColors = new Color32[Width * Height];
        protected override int SourceColorsLength => SourceColors.Length;

        protected override void StartReadSource()
        {
            if (GetPixelsBulkCount == 0)
            {
                ReadIndex = 0;
                VRCAsyncGPUReadback.Request(Source, 0, TextureFormat.RGBA32, (IUdonEventReceiver)this);
            }
            else
            {
                ReadIndex = -1;
                ReadPixels();
            }
        }

        protected override void CancelReadSource()
        {
            ReadIndex = -2;
        }

        public override void OnAsyncGpuReadbackComplete(VRCAsyncGPUReadbackRequest request)
        {
            if (ReadIndex != 0)
            {
                return;
            }
            ReadIndex = -1;
            var colors = new Color32[Width * Height];
            if (request.hasError || !request.TryGetData(colors))
            {
                Debug.LogError($"[SyncTexture] OnAsyncGpuReadbackComplete error");
                CancelSync();
                return;
            }
            StoreSourceColors(colors, 0);
            StartSyncNext();
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
                ReadIndex = -1;
                StartSyncNext();
                return;
            }
            var height = Mathf.Min(GetPixelsBulkCount, Source.height - startHeight);
            var colors = ((Texture2D)Source).GetPixels(0, startHeight, Source.width, height);
            var colors32 = new Color32[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors32[i] = colors[i];
            }
            StoreSourceColors(colors32, startHeight * Source.width);
            SendCustomEventDelayedFrames(nameof(ReadPixels), 1);
        }

        protected override void ApplyReceiveColors()
        {
            Target.SetPixels(UnpackReceiveColors());
            Target.Apply();
        }

        protected override void ApplyReceiveColorsPartial(int minHeight, int height)
        {
            var colors = UnpackReceiveColorsPartial(minHeight * Width, height * Width);
            Target.SetPixels(0, minHeight, Width, height, colors);
            Target.Apply();
        }
    }
}
