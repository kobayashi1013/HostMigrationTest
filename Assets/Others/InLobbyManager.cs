using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UniRx;
using Network;

public class InLobbyManager : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;

    /*public override void Spawned()
    {
        RunnerManager.Instance.OnPlayerJoinedCall.Subscribe(_ =>
        {
            Debug.Log("player spawn");
        });
    }*/
}
