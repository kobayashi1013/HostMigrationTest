using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Network
{
    public class RoomServer : NetworkBehaviour
    {
        public override void Spawned()
        {
            DontDestroyOnLoad(this);
        }
    }
}
