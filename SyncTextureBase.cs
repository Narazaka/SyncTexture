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
        /// <summary>
        /// If false, do nothing.
        /// </summary>
        [PublicAPI]
        [SerializeField]
        public bool SyncEnabled = true;
        [SerializeField]
        public UdonBehaviour[] CallbackListeners;

        /// <summary>
        /// Take ownership and send texture data to other players.
        /// 
        /// If not SyncEnabled, do nothing.
        /// </summary>
        /// <returns>
        /// actually started or not
        /// </returns>
        [PublicAPI]
        public abstract bool StartSync();
        /// <summary>
        /// Take ownership and stop sending.
        /// </summary>
        /// <returns>
        /// actually canceled or not
        /// </returns>
        [PublicAPI]
        public abstract bool CancelSync();
        /// <summary>
        /// Take ownership and force start sending.
        /// 
        /// If already sending, abort and restart sending.
        /// If not SyncEnabled, do nothing.
        /// </summary>
        /// <returns>
        /// actually started or not
        /// </returns>
        [PublicAPI]
        public abstract bool ForceStartSync();
    }
}