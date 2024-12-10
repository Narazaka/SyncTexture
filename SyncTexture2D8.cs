using UdonSharp;
using UnityEngine;
using System;

namespace net.narazaka.vrchat.sync_texture
{
    [AddComponentMenu("Sync Texture/Sync Texture 2D8")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SyncTexture2D8 : SyncTexture2DTyped
    {
        [SerializeField]
        public ColorEncoder8 ColorEncoder;
        [SerializeField]
        SyncTextureData8[] DataList;

        public override int UnitByteLength => 1;
        protected override int PackUnitLength => ColorEncoder.PackUnitLength;

        byte[] sendData;

        protected override void InitializeSendData(int pixelLength)
        {
            sendData = new byte[pixelLength * PackUnitLength];
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

        byte[] data = new byte[0];

        public void ApplyReceiveColorsPartial(SyncTextureData8 data)
        {
            Debug.Log($"[SyncTexture] ({name}) ApplyReceiveColorsPartial() [ReceiveEnabled={ReceiveEnabled}]");
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

        protected void PushData(byte[] pushData)
        {
            var newData = new byte[data.Length + pushData.Length];
            data.CopyTo(newData, 0);
            pushData.CopyTo(newData, data.Length);
            data = newData;
        }

        protected override void ShiftData(int count)
        {
            var newData = new byte[data.Length - count * Width * PackUnitLength];
            Array.Copy(data, count * Width * PackUnitLength, newData, 0, newData.Length);
            data = newData;
        }

        protected override void ClearData() => data = new byte[0];
    }
}
