using UdonSharp;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture.color_encoder
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ColorEncoderRGBA4444 : ColorEncoder16
    {
        public override int PackUnitLength { get => 1; }

        public override ushort[] Pack(Color32[] colors)
        {
            var len = colors.Length;
            var data = new ushort[len];
            Pack(colors, 0, data, 0, len);
            return data;
        }

        public override void Pack(Color32[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                data[startPixelIndex + i] = EncodeRGBA4444(colors[startColorIndex + i]);
            }
        }

        public override Color[] Unpack(ushort[] data)
        {
            var len = data.Length;
            var colors = new Color[len];
            Unpack(data, 0, colors, 0, len);
            return colors;
        }

        public override Color[] Unpack(ushort[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                colors[startColorIndex + i] = DecodeRGBA4444(data[startPixelIndex + i]);
            }
            return colors;
        }

        static ushort EncodeRGBA4444(Color32 color)
        {
            return (ushort)((color.r >> 4) << 12 | (color.g >> 4) << 8 | (color.b >> 4) << 4 | (color.a >> 4));
        }

        static Color DecodeRGBA4444(ushort color)
        {
            return new Color32(
                (byte)((color >> 12) << 4),
                (byte)(((color >> 8) & 0xF) << 4),
                (byte)(((color >> 4) & 0xF) << 4),
                (byte)((color & 0xF) << 4)
            );
        }
    }
}
