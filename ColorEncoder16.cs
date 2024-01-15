using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using net.narazaka.vrchat.sync_texture.color_encoder16;

namespace net.narazaka.vrchat.sync_texture
{
    public class ColorEncoder16
    {
        public static int PackUnitLength(SendFormat16 sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat16.RGB565:
                    return ColorEncoderRGB565.PackUnitLength;
                case SendFormat16.R16G16B16A16:
                    return ColorEncoderR16G16B16A16.PackUnitLength;
                case SendFormat16.R16:
                    return ColorEncoderR16.PackUnitLength;
                default:
                    return 0;
            }
        }

        public static ushort[] Pack(Color[] colors, SendFormat16 sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat16.RGB565:
                    return ColorEncoderRGB565.Pack(colors);
                case SendFormat16.R16G16B16A16:
                    return ColorEncoderR16G16B16A16.Pack(colors);
                case SendFormat16.R16:
                    return ColorEncoderR16.Pack(colors);
                default:
                    return null;
            }
        }

        public static void Pack(Color[] colors, int startColorIndex, ushort[] data, int startPixelIndex, int pixelLength, SendFormat16 sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat16.RGB565:
                    ColorEncoderRGB565.Pack(colors, startColorIndex, data, startPixelIndex, pixelLength);
                    break;
                case SendFormat16.R16G16B16A16:
                    ColorEncoderR16G16B16A16.Pack(colors, startColorIndex, data, startPixelIndex, pixelLength);
                    break;
                case SendFormat16.R16:
                    ColorEncoderR16.Pack(colors, startColorIndex, data, startPixelIndex, pixelLength);
                    break;
            }
        }

        public static Color[] Unpack(ushort[] data, SendFormat16 sendFormat)
        {
            switch (sendFormat)
            {
                case SendFormat16.RGB565:
                    return ColorEncoderRGB565.Unpack(data);
                case SendFormat16.R16G16B16A16:
                    return ColorEncoderR16G16B16A16.Unpack(data);
                case SendFormat16.R16:
                    return ColorEncoderR16.Unpack(data);
                default:
                    return null;
            }
        }
    }
}
