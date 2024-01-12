using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture
{
    public class ColorEncoder
    {
        public static int PackUnitLength(SendFormat sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat.RGB565:
                    return 1;
                case SendFormat.R16G16B16A16:
                    return 4;
                case SendFormat.R16:
                    return 1;
                default:
                    return 0;
            }
        }

        public static ushort[] Pack(Color[] colors, SendFormat sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat.RGB565:
                    return PackRGB565(colors);
                case SendFormat.R16G16B16A16:
                    return PackR16G16B16A16(colors);
                case SendFormat.R16:
                    return PackR16(colors);
                default:
                    return null;
            }
        }

        public static void Pack(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength, SendFormat sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat.RGB565:
                    PackRGB565(colors, startColorIndex, data, startPixelIndex, pixelLength);
                    break;
                case SendFormat.R16G16B16A16:
                    PackR16G16B16A16(colors, startColorIndex, data, startPixelIndex, pixelLength);
                    break;
                case SendFormat.R16:
                    PackR16(colors, startColorIndex, data, startPixelIndex, pixelLength);
                    break;
            }
        }

        public static ushort[] PackRGB565(Color[] colors)
        {
            var len = colors.Length;
            var data = new ushort[len];
            PackRGB565(colors, 0, data, 0, len);
            return data;
        }

        public static void PackRGB565(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                data[startPixelIndex + i] = EncodeRGB565(colors[startColorIndex + i]);
            }
        }

        public static ushort[] PackR16G16B16A16(Color[] colors)
        {
            var len = colors.Length;
            var data = new ushort[len * 4];
            PackR16G16B16A16(colors, 0, data, 0, len);
            return data;
        }

        public static void PackR16G16B16A16(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength)
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

        public static ushort[] PackR16(Color[] colors)
        {
            var len = colors.Length;
            var data = new ushort[len];
            PackR16(colors, 0, data, 0, len);
            return data;
        }

        public static void PackR16(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                data[startPixelIndex + i] = (ushort)(colors[startColorIndex + i].r * ushort.MaxValue);
            }
        }

        public static Color[] Unpack(ushort[] data, SendFormat sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat.RGB565:
                    return UnpackRGB565(data);
                case SendFormat.R16G16B16A16:
                    return UnpackR16G16B16A16(data);
                case SendFormat.R16:
                    return UnpackR16(data);
                default:
                    return null;
            }
        }

        public static Color[] UnpackRGB565(ushort[] data)
        {
            var len = data.Length;
            var colors = new Color[len];
            UnpackRGB565(data, 0, colors, 0, len);
            return colors;
        }

        public static void UnpackRGB565(ushort[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                colors[startColorIndex + i] = DecodeRGB565(data[startPixelIndex + i]);
            }
        }

        public static Color[] UnpackR16G16B16A16(ushort[] data)
        {
            var len = data.Length / 4;
            var colors = new Color[len];
            UnpackR16G16B16A16(data, 0, colors, 0, len);
            return colors;
        }

        public static void UnpackR16G16B16A16(ushort[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength)
        {
            var rate = 1f / ushort.MaxValue;
            for (int i = 0; i < pixelLength; i++)
            {
                var index = (startPixelIndex + i) * 4;
                colors[startColorIndex + i] = new Color(data[index] * rate, data[index + 1] * rate, data[index + 2] * rate, data[index + 3] * rate);
            }
        }

        public static Color[] UnpackR16(ushort[] data)
        {
            var len = data.Length;
            var colors = new Color[len];
            UnpackR16(data, 0, colors, 0, len);
            return colors;
        }

        public static void UnpackR16(ushort[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength)
        {
            var rate = 1f / ushort.MaxValue;
            for (int i = 0; i < pixelLength; i++)
            {
                colors[startColorIndex + i] = new Color(data[startPixelIndex + i] * rate, 0, 0, 1);
            }
        }

        public static ushort EncodeRGB565(Color color)
        {
            return (ushort)((ushort)(color.r * 31.0) << 11 | (ushort)(color.g * 63.0) << 5 | (ushort)(color.b * 31.0));
        }

        public static Color DecodeRGB565(ushort color)
        {
            return new Color(
                1f * (color >> 11) / 0x1F,
                1f * ((color >> 5) & 0x3F) / 0x3F,
                1f * (color & 0x1F) / 0x1F
            );
        }
    }
}
