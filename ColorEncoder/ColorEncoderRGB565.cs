using UdonSharp;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture.color_encoder
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ColorEncoderRGB565 : ColorEncoder16
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
                data[startPixelIndex + i] = EncodeRGB565(colors[startColorIndex + i]);
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
                colors[startColorIndex + i] = DecodeRGB565(data[startPixelIndex + i]);
            }
            return colors;
        }

        static ushort EncodeRGB565(Color32 color)
        {
            return (ushort)((color.r >> 3) << 11 | (color.g >> 2) << 5 | (color.b >> 3));
        }

        static Color DecodeRGB565(ushort color)
        {
            return new Color(
                1f * (color >> 11) / 0x1F,
                1f * ((color >> 5) & 0x3F) / 0x3F,
                1f * (color & 0x1F) / 0x1F
            );
        }
    }
}
