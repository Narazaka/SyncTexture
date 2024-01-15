
using UdonSharp;
using UnityEngine;
using System;
using VRC.Udon;


namespace net.narazaka.vrchat.sync_texture
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTexture2D16 : SyncTexture
    {
        [SerializeField]
        public SendFormat16 SendFormat;
        [UdonSynced]
        ushort[] SyncColors;
        ushort[] Colors = new ushort[0];
        ushort[] ReceiveColors = new ushort[0];

        protected override int PackUnitLength => ColorEncoder16.PackUnitLength(SendFormat);
        protected override void PackColors(Color[] colors, int startColorIndex, int startPixelIndex, int pixelLength)
        {
            ColorEncoder16.Pack(colors, 0, Colors, startPixelIndex, pixelLength, SendFormat);
        }
        protected override Color[] UnpackReceiveColors()
        {
            return ColorEncoder16.Unpack(ReceiveColors, SendFormat);
        }
        protected override Color[] UnpackReceiveColorsPartial(int startReceiveIndex, int length)
        {
            var partColors = new ushort[length];
            Array.Copy(ReceiveColors, startReceiveIndex, partColors, 0, length);
            return ColorEncoder16.Unpack(partColors, SendFormat);
        }

        protected override void InitializeSyncColors(int size)
        {
            SyncColors = new ushort[size];
        }

        protected override void InitializeSourceColors(int size)
        {
            Colors = new ushort[PackUnitLength * size];
        }

        protected override void InitializeReceiveColors(int size)
        {
            ReceiveColors = new ushort[PackUnitLength * size];
        }

        protected override int SourceColorsLength => Colors.Length;
        protected override bool ReceiveColorsIsEmpty => ReceiveColors == null || ReceiveColors.Length == 0;
        protected override bool ReceiveColorsIsValid(int size) => ReceiveColors != null && ReceiveColors.Length == PackUnitLength * size;

        protected override void CopySourceColorsToSyncColors(int startSourceIndex, int length)
        {
            Array.Copy(Colors, startSourceIndex, SyncColors, 0, length);
        }

        protected override void CopySyncColorsToReceiveColors(int startReceiveIndex)
        {
            Array.Copy(SyncColors, 0, ReceiveColors, startReceiveIndex, SyncColors.Length);
        }
    }
}
