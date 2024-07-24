using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;

namespace Prefabs
{
    public class PlayerInfo : NetworkBehaviour
    {
        [Networked] public int connectionToken { get; set; }
        [Networked] public bool snapshot { get; set; }
        [Networked] public Vector3 position { get; set; }
        [Networked] public  Quaternion rotation { get; set; }

        public void SetSnapshot(bool state)
        {
            //Debug.Log($"SetSnapshot({state})");
            snapshot = state;
        }

        public override void Render()
        {
            if (this.Object.HasStateAuthority)
            {
                position = transform.position;
                rotation = transform.rotation;
            }
        }
    }
}
