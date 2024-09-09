using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UniRx;
using Network;

public class InLobbyManager : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;

    public override void Spawned()
    {
        if (!Runner.IsServer) return;

        RunnerManager.Instance.OnPlayerSpawnedCall.Subscribe(player =>
        {
            var playerObj = RunnerManager.Instance.PlayerSpawned(
                _playerPrefab,
                new Vector3(0, Random.Range(0, 100), 0),
                Quaternion.identity,
                player);
            RunnerManager.Instance.PlayerList.Add(player, playerObj);
        }).AddTo(this);
    }
}
