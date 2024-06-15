
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SyncTexturesOnLateJoin : UdonSharpBehaviour
    {
        [SerializeField]
        SyncTextureManager SyncTextureManager;
        [SerializeField]
        float Delay = 8f;

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal) return;
            SendCustomEventDelayedSeconds(nameof(StartSync), Delay);
        }

        public void StartSync()
        {
            if (!Networking.IsOwner(SyncTextureManager.gameObject)) return;
            SyncTextureManager.StartSyncAll(true);
        }
    }
}
