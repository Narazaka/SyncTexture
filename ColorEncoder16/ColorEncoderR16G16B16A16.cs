using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using net.narazaka.vrchat.sync_texture.color_encoder16;

namespace net.narazaka.vrchat.sync_texture.color_encoder16
{
    public partial class ColorEncoderR16G16B16A16
    {
        public const int PackUnitLength = 4;

        public static ushort[] Pack(Color[] colors)
        {
            var len = colors.Length;
            var data = new ushort[len * 4];
            Pack(colors, 0, data, 0, len);
            return data;
        }

        public static void Pack(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                var color = colors[startColorIndex + i] * ushort.MaxValue;
                var index = (startPixelIndex + i) * 4;
                data[index] = (ushort)color.r;
                data[index + 1] = (ushort)color.g;
                data[index + 2] = (ushort)color.b;
                data[index + 3] = (ushort)color.a;
            }
        }

        public static Color[] Unpack(ushort[] data)
        {
            var len = data.Length / 4;
            var colors = new Color[len];
            Unpack(data, 0, colors, 0, len);
            return colors;
        }

        public static void Unpack(ushort[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength)
        {
            var rate = 1f / ushort.MaxValue;
            for (int i = 0; i < pixelLength; i++)
            {
                var index = (startPixelIndex + i) * 4;
                colors[startColorIndex + i] = new Color(data[index] * rate, data[index + 1] * rate, data[index + 2] * rate, data[index + 3] * rate);
            }
        }
    }
}
