
using UdonSharp;
using UnityEngine;
using System;
using VRC.Udon;


namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTexture2D : SyncTexture
    {
        [SerializeField]
        public Texture2D Source;
        [SerializeField]
        public Texture2D Target;
        [SerializeField]
        public int GetPixelsBulkCount = 8;

        int ReadIndex = -1;
        protected override bool ReadingSource => ReadIndex >= 0;

        protected override int Width => Source.width;
        protected override int Height => Source.height;

        abstract protected void PackColors(Color[] colors, int startColorIndex, int startPixelIndex, int pixelLength);
        abstract protected Color[] UnpackReceiveColors();
        abstract protected Color[] UnpackReceiveColorsPartial(int startReceiveIndex, int length);


        protected override void StartReadSource()
        {
            ReadIndex = -1;
            ReadPixels();
        }

        protected override void CancelReadSource()
        {
            ReadIndex = -2;
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
            var colors = Source.GetPixels(0, startHeight, Source.width, height);
            PackColors(colors, 0, startHeight * Source.width, colors.Length);
            SendCustomEventDelayedFrames(nameof(ReadPixels), 1);
        }

        protected override void ApplyReceiveColors()
        {
            Target.SetPixels(UnpackReceiveColors());
            Target.Apply();
        }

        protected override void ApplyReceiveColorsPartial(int minHeight, int height)
        {
            var colors = UnpackReceiveColorsPartial(minHeight * Width * PackUnitLength, height * Width * PackUnitLength);
            Target.SetPixels(0, minHeight, Width, height, colors);
            Target.Apply();
        }
    }
}
