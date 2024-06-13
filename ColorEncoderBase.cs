using UdonSharp;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture
{
    /// <summary>
    /// Pack / unpack color data to / from suitable binary array.
    /// </summary>
    /// <typeparam name="T">binary unit (Int8, Int16 etc)</typeparam>
    public abstract class ColorEncoderBase<T> : UdonSharpBehaviour where T : struct
    {
        public abstract int PackUnitLength { get; }

        public abstract T[] Pack(Color32[] colors);
        public abstract void Pack(Color32[] colors, int startColorIndex, T[] data, int startPixelIndex, int pixelLength);

        public abstract Color[] Unpack(T[] data);

        public abstract Color[] Unpack(T[] data, int startPixelIndex, Color[] colors, int startColorIndex, int pixelLength);
    }
}
