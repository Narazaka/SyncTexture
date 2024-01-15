using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using net.narazaka.vrchat.sync_texture.color_encoder16;

namespace net.narazaka.vrchat.sync_texture.color_encoder16
{
    public partial class ColorEncoderR16
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
                data[startPixelIndex + i] = (ushort)(colors[startColorIndex + i].r * ushort.MaxValue);
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
            var rate = 1f / ushort.MaxValue;
            for (int i = 0; i < pixelLength; i++)
            {
                colors[startColorIndex + i] = new Color(data[startPixelIndex + i] * rate, 0, 0, 1);
            }
        }
    }
}
