using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture.color_encoder.test
{
    public class ColorEncoderTest
    {
        [TestCase(typeof(ColorEncoderR8), "Packages/net.narazaka.vrchat.sync-texture/Test/Textures/R.png")]
        [TestCase(typeof(ColorEncoderRG88), "Packages/net.narazaka.vrchat.sync-texture/Test/Textures/RG.png")]
        [TestCase(typeof(ColorEncoderRGB888), "Packages/net.narazaka.vrchat.sync-texture/Test/Textures/RGB.png")]
        [TestCase(typeof(ColorEncoderRGBA8888), "Packages/net.narazaka.vrchat.sync-texture/Test/Textures/RGBA.png")]
        public void PackTest8(System.Type component, string texturePath)
        {
            var go = new GameObject();
            var colorEncoder = go.AddComponent(component) as ColorEncoder8;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            var colors = texture.GetPixels32();
            var data = colorEncoder.Unpack(colorEncoder.Pack(colors));
            Assert.AreEqual(colors.Length, data.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                Assert.AreEqual((Color)colors[i], data[i]);
            }
        }

        [Test]
        public void PackTestRGB565()
        {
            var go = new GameObject();
            var colorEncoder = go.AddComponent<ColorEncoderRGB565>();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/net.narazaka.vrchat.sync-texture/Test/Textures/RGB.png");
            var colors = texture.GetPixels32();
            var data = colorEncoder.Unpack(colorEncoder.Pack(colors));
            Assert.AreEqual(colors.Length, data.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                Assert.AreEqual((Color)RGB565(colors[i]), data[i]);
            }
        }

        Color32 RGB565(Color32 color) => new Color32(
            (byte)((color.r >> 3) << 3),
            (byte)((color.g >> 2) << 2),
            (byte)((color.b >> 3) << 3),
            0xFF
        );

        [Test]
        public void PackTestRGBA4444()
        {
            var go = new GameObject();
            var colorEncoder = go.AddComponent<ColorEncoderRGBA4444>();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/net.narazaka.vrchat.sync-texture/Test/Textures/RGBA.png");
            var colors = texture.GetPixels32();
            var data = colorEncoder.Unpack(colorEncoder.Pack(colors));
            Assert.AreEqual(colors.Length, data.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                Assert.AreEqual((Color)RGBA4444(colors[i]), data[i]);
            }
        }

        Color32 RGBA4444(Color32 color) => new Color32(
            (byte)((color.r >> 4) << 4),
            (byte)((color.g >> 4) << 4),
            (byte)((color.b >> 4) << 4),
            (byte)((color.a >> 4) << 4)
        );
    }
}
