using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture.color_encoder16
{
    public class ColorEncoderRGBA4444
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
                data[startPixelIndex + i] = EncodeRGBA4444(colors[startColorIndex + i]);
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
                colors[startColorIndex + i] = DecodeRGBA4444(data[startPixelIndex + i]);
            }
        }

        static ushort EncodeRGBA4444(Color color)
        {
            return (ushort)((ushort)(color.r * 15.0) << 12 | (ushort)(color.g * 15.0) << 8 | (ushort)(color.b * 15.0) << 4 | (ushort)(color.a * 15.0));
        }

        static Color DecodeRGBA4444(ushort color)
        {
            return new Color(
                1f * (color >> 12) / 0xF,
                1f * ((color >> 8) & 0xF) / 0xF,
                1f * ((color >> 4) & 0xF) / 0xF,
                1f * (color & 0xF) / 0xF
            );
        }
    }
}
