using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Network;

namespace Prefabs
{
    [RequireComponent(typeof(ObjectToken))]
    public class PlayerInfo : NetworkBehaviour
    {
        [Networked] public int connectionToken { get; set; }
    }
}
