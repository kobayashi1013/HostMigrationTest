using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Network;
using Interface;
using Prefabs;
using Constant;

namespace Scenes.InGame
{
    public class InGameManager : NetworkBehaviour, ISceneManager
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;

        public override void Render()
        {
            if (this.Object.HasStateAuthority)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    NetworkManager.Runner.LoadScene(SceneRef.FromIndex((int)SceneName.InLobby));
                }
            }
        }

        /// <summary>
        /// プレイヤースポーン
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="player"></param>
        public NetworkObject SpawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            //MigrationTest用の実装
            var position = new Vector3(UnityEngine.Random.Range(0, 100), 0, 0);

            var playerObj = runner.Spawn(
                _playerPrefab,
                position,
                Quaternion.identity,
                player,
                (_, obj) =>
                {
                    var playerInfo = obj.GetComponent<PlayerInfo>();
                    playerInfo.connectionToken = new Guid(runner.GetPlayerConnectionToken(player)).GetHashCode();

                    if (player.PlayerId == runner.LocalPlayer.PlayerId) playerInfo.SetSnapshot(false);
                    else playerInfo.SetSnapshot(true);
                });

            return playerObj;
        }

        public async Task DespawnPlayer(NetworkRunner runner, NetworkObject playerObj)
        {
            var playerInfo = playerObj.GetComponent<PlayerInfo>();
            playerInfo.SetSnapshot(false);

            await Task.Delay(10000);

            runner.Despawn(playerObj);
        }
    }
}