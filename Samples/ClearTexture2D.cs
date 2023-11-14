
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture.samples
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ClearTexture2D : UdonSharpBehaviour
    {
        [SerializeField]
        Texture2D[] Textures;

        public void ClearTexture()
        {
            foreach (var texture in Textures)
            {
                var colors = new Color[texture.width * texture.height];
                texture.SetPixels(colors);
                texture.Apply();
            }
        }
    }
}
