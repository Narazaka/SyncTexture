
using UdonSharp;
using UnityEngine;
using System;
using VRC.Udon;


namespace net.narazaka.vrchat.sync_texture
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTexture2D16 : SyncTexture2D
    {
        [SerializeField]
        public ColorEncoder16 ColorEncoder;
        [UdonSynced]
        ushort[] SyncColors;
        ushort[] ReceiveColors = new ushort[0];

        protected override int PackUnitLength => ColorEncoder.PackUnitLength;
        protected override Color[] UnpackReceiveColors() =>  ColorEncoder.Unpack(ReceiveColors);
        protected override Color[] UnpackReceiveColorsPartial(int startReceivePixelIndex, int pixelLength) => ColorEncoder.Unpack(ReceiveColors, startReceivePixelIndex, new Color[pixelLength], 0, pixelLength);
        protected override void InitializeSyncColors(int size) => SyncColors = new ushort[size];
        protected override void InitializeReceiveColors() => ReceiveColors = new ushort[PackUnitLength * Width * Height];
        protected override bool ReceiveColorsIsEmpty => ReceiveColors == null || ReceiveColors.Length == 0;
        protected override bool ReceiveColorsIsValid => ReceiveColors != null && ReceiveColors.Length == PackUnitLength * Width * Height;
        protected override void CopySourceColorsToSyncColors(int startSourceIndex, int length) => ColorEncoder.Pack(SourceColors, startSourceIndex, SyncColors, 0, length);
        protected override void CopySyncColorsToReceiveColors(int startReceiveIndex) => Array.Copy(SyncColors, 0, ReceiveColors, startReceiveIndex, SyncColors.Length);
    }
}
