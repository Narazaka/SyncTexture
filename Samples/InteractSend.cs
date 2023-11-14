
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture.samples
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InteractSend : UdonSharpBehaviour
    {
        [SerializeField]
        UdonSharpBehaviour Target;
        [SerializeField]
        string EventName;

        public override void Interact()
        {
            Target.SendCustomEvent(EventName);
        }
    }
}
