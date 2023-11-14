using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTextureCallbackListener : UdonSharpBehaviour
    {
        public abstract void OnPreSync();
        public abstract void OnSyncStart();
        public abstract void OnSync();
        public abstract void OnSyncComplete();
        public abstract void OnSyncCanceled();
        public abstract void OnReceiveStart();
        public abstract void OnReceive();
        public abstract void OnReceiveComplete();
        public abstract void OnReceiveCanceled();
    }
}
