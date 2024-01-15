using UdonSharp;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class ColorEncoderBase<T> : UdonSharpBehaviour where T : struct
    {
        public abstract int PackUnitLength { get; }

        public abstract T[] Pack(Color32[] colors);
        public abstract void Pack(Color32[] colors, int startColorIndex, T[] data, int startPixelIndex, int pixelLength);

        public abstract Color[] Unpack(T[] data);

        public abstract void Unpack(T[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength);
    }
}
