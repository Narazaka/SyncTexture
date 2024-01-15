﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static log4net.Appender.ColoredConsoleAppender;


namespace net.narazaka.vrchat.sync_texture
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncTexture2D16 : SyncTexture<ushort>
    {
        [SerializeField]
        public SendFormat SendFormat;

        protected override int PackUnitLength => ColorEncoder.PackUnitLength(SendFormat);
        protected override void PackColors(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength)
        {
            ColorEncoder.Pack(colors, 0, data, startPixelIndex, pixelLength, SendFormat);
        }
        protected override Color[] UnpackColors(ushort[] colors)
        {
            return ColorEncoder.Unpack(colors, SendFormat);
        }
    }
}
