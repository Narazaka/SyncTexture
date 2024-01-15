using JetBrains.Annotations;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTextureBase : UdonSharpBehaviour
    {
        [SerializeField]
        public bool SyncEnabled = true;
        [SerializeField]
        public UdonBehaviour CallbackListener;

        public abstract bool StartSync();
        public abstract bool CancelSync();
        public abstract bool ForceStartSync();
    }
}