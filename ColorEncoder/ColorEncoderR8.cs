using UdonSharp;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture.color_encoder
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ColorEncoderR8 : ColorEncoder8
    {
        public override int PackUnitLength { get => 1; }

        public override byte[] Pack(Color32[] colors)
        {
            var len = colors.Length;
            var data = new byte[len];
            Pack(colors, 0, data, 0, len);
            return data;
        }

        public override void Pack(Color32[] colors, int startColorIndex, byte[] data, int startPixelIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                data[startPixelIndex + i] = colors[startColorIndex + i].r;
            }
        }

        public override Color[] Unpack(byte[] data)
        {
            var len = data.Length;
            var colors = new Color[len];
            Unpack(data, 0, colors, 0, len);
            return colors;
        }

        public override Color[] Unpack(byte[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength)
        {
            for (int i = 0; i < pixelLength; i++)
            {
                colors[startColorIndex + i] = new Color32(data[startPixelIndex + i], 0, 0, 0xFF);
            }
            return colors;
        }
    }
}
