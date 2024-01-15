
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
        protected override void PackColors(Color[] colors, int startColorIndex, int startPixelIndex, int pixelLength)
        {
            ColorEncoder.Pack(colors, 0, Colors, startPixelIndex, pixelLength, SendFormat);
        }
        protected override Color[] UnpackReceiveColors()
        {
            return ColorEncoder.Unpack(ReceiveColors, SendFormat);
        }
        protected override Color[] UnpackReceiveColorsPartial(int startReceiveIndex, int length)
        {
            var partColors = new ushort[length];
            System.Array.Copy(ReceiveColors, startReceiveIndex, partColors, 0, length);
            return ColorEncoder.Unpack(partColors, SendFormat);
        }
    }
}
