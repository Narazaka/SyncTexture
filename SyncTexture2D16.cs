using UdonSharp;
using UnityEngine;
using System;

namespace net.narazaka.vrchat.sync_texture
{
    [AddComponentMenu("Sync Texture/Sync Texture 2D16")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SyncTexture2D16 : SyncTexture2DTyped
    {
        [SerializeField]
        public ColorEncoder16 ColorEncoder;
        [SerializeField]
        SyncTextureData16[] DataList;

        public override int UnitByteLength => 2;
        protected override int PackUnitLength => ColorEncoder.PackUnitLength;

        ushort[] sendData;

        protected override void InitializeSendData(int pixelLength)
        {
            sendData = new ushort[pixelLength * PackUnitLength];
        }

        protected override void PackColorsPartial(int startColorIndex, int startPixelIndex, int pixelLength)
        {
            ColorEncoder.Pack(SourceColors, startColorIndex, sendData, startPixelIndex, pixelLength);
        }

        protected override void DoSyncColors()
        {
            DataList[SyncIndex].Send(sendData);
#if UNITY_EDITOR
            DataList[SyncIndex].OnDeserialization();
            DataList[SyncIndex].OnPostSerialization(new VRC.Udon.Common.SerializationResult(true, 1));
#endif
        }

        ushort[] data = new ushort[0];

        public void ApplyReceiveColorsPartial(SyncTextureData16 data)
        {
            Debug.Log($"{LogPrefix} ApplyReceiveColorsPartial() [ReceiveEnabled={ReceiveEnabled}]");
            Callback(nameof(SyncTextureCallbackListener.OnReceive));
            if (!ReceiveEnabled) return;
            var syncIndex = Array.IndexOf(DataList, data);
            if (syncIndex == -1) return;
            var minHeight = syncIndex * EffectiveBulkLineCount;
            var height = Mathf.Min(EffectiveBulkLineCount, Height - minHeight);
            var toCall = linesEmpty;
            EnsureData();
            PushLines(minHeight, height);
            PushData(data.Data);
            if (toCall)
            {
                ApplyReceiveColorsPartialLine();
            }
        }

        protected override Color[] UnpackLineColors(int index) => ColorEncoder.Unpack(data, index * Width, Width);

        protected void PushData(ushort[] pushData)
        {
            var newData = new ushort[data.Length + pushData.Length];
            data.CopyTo(newData, 0);
            pushData.CopyTo(newData, data.Length);
            data = newData;
        }

        protected override void ShiftData(int count)
        {
            var newData = new ushort[data.Length - count * Width * PackUnitLength];
            Array.Copy(data, count * Width * PackUnitLength, newData, 0, newData.Length);
            data = newData;
        }

        protected override void ClearData() => data = new ushort[0];

        /*
        int[] lines = new int[0];
        ushort[] data = new ushort[0];
        int dataWidth;
        int dataHeight;
        int dataUnitByteLength;
        const int MaxBulkSetPixelLineCount = 4;


        public void ApplyReceiveColorsPartial(SyncTextureData16 data)
        {
            var syncIndex = Array.IndexOf(DataList, data);
            if (syncIndex == -1) return;
            var minHeight = syncIndex * EffectiveBulkLineCount;
            var height = Mathf.Min(EffectiveBulkLineCount, Height - minHeight);
            var toCall = lines.Length == 0;
            EnsureData();
            PushData(data.Data, minHeight, height);
            if (toCall)
            {
                ApplyReceiveColorsPartialLine();
            }
        }

        public void ApplyReceiveColorsPartialLine()
        {
            EnsureData();
            if (lines.Length == 0) return;

            var count = Mathf.Min(MaxBulkSetPixelLineCount, lines.Length);
            for (var i = 0; i < count; i++)
            {
                var minHeight = lines[i];
                var colors = ColorEncoder.Unpack(data, i * Width * PackUnitLength, Width * PackUnitLength);
                Target.SetPixels(0, minHeight, Width, 1, colors);
            }
            Target.Apply();

            var newLines = new int[lines.Length - count];
            Array.Copy(lines, count, newLines, 0, newLines.Length);
            lines = newLines;
            var newData = new ushort[data.Length - count * Width * PackUnitLength];
            Array.Copy(data, count * Width * PackUnitLength, newData, 0, newData.Length);
            data = newData;

            if (lines.Length == 0) return;

            SendCustomEventDelayedFrames(nameof(ApplyReceiveColorsPartialLine), 1);
        }

        void PushData(ushort[] pushData, int minHeight, int heightCount)
        {
            var len = lines.Length;
            var newLines = new int[len + heightCount];
            lines.CopyTo(newLines, 0);
            for (int i = 0; i < heightCount; i++)
            {
                newLines[len + i] = minHeight + i;
            }
            lines = newLines;

            var newData = new ushort[data.Length + pushData.Length];
            data.CopyTo(newData, 0);
            pushData.CopyTo(newData, data.Length);
            data = newData;
        }

        void EnsureData()
        {
            if (dataWidth != Width || dataHeight != Height || dataUnitByteLength != UnitByteLength)
            {
                lines = new int[0];
                ClearData();
            }
            dataWidth = Width;
            dataHeight = Height;
            dataUnitByteLength = UnitByteLength;
        }

        void ClearData()
        {
            data = new ushort[0];
        }
        */
    }
}
