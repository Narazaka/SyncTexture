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
        public UdonBehaviour[] CallbackListeners;

        /// <summary>
        /// receive texture data from other players.
        /// </summary>
        [PublicAPI]
        public bool ReceiveEnabled = true;
        /// <summary>
        /// send texture data to other players.
        /// </summary>
        /// <returns>
        /// actually started or not
        /// </returns>
        [PublicAPI]
        public abstract bool StartSync();
        /// <summary>
        /// stop sending.
        /// </summary>
        /// <returns>
        /// actually canceled or not
        /// </returns>
        [PublicAPI]
        public abstract bool CancelSync();
        /// <summary>
        /// force start sending.
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