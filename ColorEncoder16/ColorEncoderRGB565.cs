using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture.color_encoder16
{
    public class ColorEncoderRGB565
    {
        public const int PackUnitLength = 1;

        public static ushort[] Pack(Color[] colors)
        {
            var len = colors.Length;
            var data = new ushort[len];
            Pack(colors, 0, data, 0, len);
            return data;
        }

        public static void Pack(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                data[startPixelIndex + i] = EncodeRGB565(colors[startColorIndex + i]);
            }
        }

        public static Color[] Unpack(ushort[] data)
        {
            var len = data.Length;
            var colors = new Color[len];
            Unpack(data, 0, colors, 0, len);
            return colors;
        }

        public static void Unpack(ushort[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                colors[startColorIndex + i] = DecodeRGB565(data[startPixelIndex + i]);
            }
        }

        static ushort EncodeRGB565(Color color)
        {
            return (ushort)((ushort)(color.r * 31.0) << 11 | (ushort)(color.g * 63.0) << 5 | (ushort)(color.b * 31.0));
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
