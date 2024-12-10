using UnityEngine;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class ColorEncoder8 : ColorEncoderBase<byte>
    {
        public override byte[] Pack(Color32[] colors, int startColorIndex, int pixelLength)
        {
            var data = new byte[pixelLength * PackUnitLength];
            Pack(colors, startColorIndex, data, 0, pixelLength);
            return data;
        }

        public override Color[] Unpack(byte[] data, int startPixelIndex, int pixelLength)
        {
            var colors = new Color[pixelLength];
            return Unpack(data, startPixelIndex, colors, 0, pixelLength);
        }
    }
}
