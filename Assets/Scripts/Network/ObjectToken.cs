using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Network
{
    public class ObjectToken : NetworkBehaviour
    {
        [Networked] public string token { get; set; }
        [Networked] public Vector3 position { get; set; } = Vector3.zero;
        [Networked] public Quaternion rotation { get; set; } = Quaternion.identity;

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsServer)
            {
                position = transform.position;
                rotation = transform.rotation;
            }
        }
    }
}
