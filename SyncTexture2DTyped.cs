using System;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTexture2DTyped : SyncTexture2D
    {
        protected abstract void InitializeSendData(int pixelLength);
        protected abstract void PackColorsPartial(int startColorIndex, int startPixelIndex, int pixelLength);
        protected abstract void DoSyncColors();

        int packPixelOffset = -1;
        int startSourceIndex;
        int pixelLength;
        const int MaxBulkPackPixelCount = 4096;

        protected override void SyncColors(int startSourceIndex, int pixelLength)
        {
            InitializeSendData(pixelLength);
            packPixelOffset = 0;
            this.startSourceIndex = startSourceIndex;
            this.pixelLength = pixelLength;
            PackColorsPartial();
        }

        public void PackColorsPartial()
        {
            var len = Mathf.Min(MaxBulkPackPixelCount, pixelLength - packPixelOffset);
            PackColorsPartial(startSourceIndex + packPixelOffset, packPixelOffset, len);
            packPixelOffset += len;
            if (packPixelOffset < pixelLength)
            {
                SendCustomEventDelayedFrames(nameof(PackColorsPartial), 1);
            }
            else
            {
                DoSyncColors();
            }
        }

        protected abstract void ClearData();
        protected abstract void ShiftData(int count);
        protected abstract Color[] UnpackLineColors(int index);

        int[] lineIndexes = new int[0];
        protected bool linesEmpty => lineIndexes.Length == 0;
        int dataWidth;
        int dataHeight;
        int dataUnitByteLength;
        const int MaxBulkSetPixelLineCount = 4;

        public void ApplyReceiveColorsPartialLine()
        {
            EnsureData();
            if (linesEmpty) return;

            var count = Mathf.Min(MaxBulkSetPixelLineCount, lineIndexes.Length);
            for (var i = 0; i < count; i++)
            {
                var minHeight = lineIndexes[i];
                var colors = UnpackLineColors(i);
                Target.SetPixels(0, minHeight, Width, 1, colors);
            }
            Target.Apply();

            ShiftLines(count);
            ShiftData(count);

            if (linesEmpty) return;

            SendCustomEventDelayedFrames(nameof(ApplyReceiveColorsPartialLine), 1);
        }

        protected void PushLines(int minHeight, int heightCount)
        {
            var len = lineIndexes.Length;
            var newLines = new int[len + heightCount];
            lineIndexes.CopyTo(newLines, 0);
            for (int i = 0; i < heightCount; i++)
            {
                newLines[len + i] = minHeight + i;
            }
            lineIndexes = newLines;
        }

        void ShiftLines(int count)
        {
            var newLines = new int[lineIndexes.Length - count];
            Array.Copy(lineIndexes, count, newLines, 0, newLines.Length);
            lineIndexes = newLines;
        }

        protected void EnsureData()
        {
            if (dataWidth != Width || dataHeight != Height || dataUnitByteLength != UnitByteLength)
            {
                lineIndexes = new int[0];
                ClearData();
            }
            dataWidth = Width;
            dataHeight = Height;
            dataUnitByteLength = UnitByteLength;
        }
    }
}
