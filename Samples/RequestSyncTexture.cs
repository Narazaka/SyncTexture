using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture.samples
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RequestSyncTexture : UdonSharpBehaviour
    {
        [SerializeField] SyncTextureManager syncTextureManager;
        [SerializeField] SyncTexture syncTexture;
        [SerializeField] bool resendWhenExistsAndNowSending = true;

        public override void Interact()
        {
            Send();
        }

        public void Send()
        {
            syncTextureManager.RequestSyncTexture(syncTexture, resendWhenExistsAndNowSending);
        }
    }
}
