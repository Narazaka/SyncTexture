using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdonSharp;

namespace net.narazaka.vrchat.sync_texture
{
    public abstract class SyncTexturePrepareCallbackListener : UdonSharpBehaviour
    {
        public abstract void OnPrepare();
        public abstract void OnPrepareCancel();
    }
}
