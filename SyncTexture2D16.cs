
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
        Color32[] Colors = new Color32[0];
        ushort[] ReceiveColors = new ushort[0];

        protected override int PackUnitLength => ColorEncoder.PackUnitLength;
        protected override void StoreColors(Color32[] colors, int startPixelIndex)
        {
            Array.Copy(colors, 0, Colors, startPixelIndex, colors.Length);
        }
        protected override Color[] UnpackReceiveColors()
        {
            return ColorEncoder.Unpack(ReceiveColors);
        }
        protected override Color[] UnpackReceiveColorsPartial(int startReceiveIndex, int length)
        {
            var partColors = new ushort[length];
            Array.Copy(ReceiveColors, startReceiveIndex, partColors, 0, length);
            return ColorEncoder.Unpack(partColors);
        }

        protected override void InitializeSyncColors(int size)
        {
            SyncColors = new ushort[size];
        }

        protected override void InitializeSourceColors(int size)
        {
            Colors = new Color32[PackUnitLength * size];
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
            ColorEncoder.Pack(Colors, startSourceIndex, SyncColors, 0, length);
        }

        protected override void CopySyncColorsToReceiveColors(int startReceiveIndex)
        {
            Array.Copy(SyncColors, 0, ReceiveColors, startReceiveIndex, SyncColors.Length);
        }
    }
}
