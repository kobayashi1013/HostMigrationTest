using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Network;
using Prefabs;
using Utils;
using Constant;

namespace Scenes.InGame
{
    [RequireComponent(typeof(ObjectToken))]
    public class InGameManager : NetworkBehaviour, ISceneManager
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;

        public override void Spawned()
        {
            if (Runner.IsServer && !Runner.IsResume)
            {
                var playerList = new List<PlayerRef>(SessionInfoCache.Instance.playerList.Keys);
                foreach (var player in playerList)
                {
                    var playerObj = SpawnPlayer(Runner, player);
                    SessionInfoCache.Instance.playerList[player] = playerObj;
                }
            }
        }

        public override void Render()
        {
            if (Runner.IsServer)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    NetworkManager.Runner.LoadScene(SceneRef.FromIndex((int)SceneName.InLobby));
                }
            }
        }

        public NetworkObject SpawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            var position = new Vector3(UnityEngine.Random.Range(0, 100), 0, 0);

            var playerObj = runner.Spawn(
                _playerPrefab,
                position,
                Quaternion.identity,
                player,
                (_, obj) =>
                {
                    var playerInfo = obj.GetComponent<PlayerInfo>();
                    var objectTokenCs = obj.GetComponent<ObjectToken>();
                    var objectToken = "token";
                    if (runner.LocalPlayer == player) objectToken = "HOST";
                    else objectToken = Guid.NewGuid().ToString();

                    playerInfo.connectionToken = new Guid(runner.GetPlayerConnectionToken(player)).GetHashCode();
                    objectTokenCs.token = objectToken;
                });

            return playerObj;
        }
    }
}
