using UdonSharp;
using UnityEngine;
using System;

namespace net.narazaka.vrchat.sync_texture
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTexture2D8 : SyncTexture2D
    {
        [SerializeField]
        public ColorEncoder8 ColorEncoder;
        [UdonSynced]
        byte[] SyncColors;
        byte[] ReceiveColors = new byte[0];

        protected override int PackUnitLength => ColorEncoder.PackUnitLength;
        protected override Color[] UnpackReceiveColors() => ColorEncoder.Unpack(ReceiveColors);
        protected override Color[] UnpackReceiveColorsPartial(int startReceivePixelIndex, int pixelLength) => ColorEncoder.Unpack(ReceiveColors, startReceivePixelIndex, new Color[pixelLength], 0, pixelLength);
        protected override void InitializeSyncColors(int pixelLength) => SyncColors = new byte[pixelLength * PackUnitLength];
        protected override void InitializeReceiveColors() => ReceiveColors = new byte[PackUnitLength * Width * Height];
        protected override bool ReceiveColorsIsEmpty => ReceiveColors == null || ReceiveColors.Length == 0;
        protected override bool ReceiveColorsIsValid => ReceiveColors != null && ReceiveColors.Length == PackUnitLength * Width * Height;
        protected override void CopySourceColorsToSyncColors(int startSourceIndex, int pixelLength) => ColorEncoder.Pack(SourceColors, startSourceIndex, SyncColors, 0, pixelLength);
        protected override void CopySyncColorsToReceiveColors(int startReceiveIndex) => Array.Copy(SyncColors, 0, ReceiveColors, startReceiveIndex, SyncColors.Length);
    }
}
