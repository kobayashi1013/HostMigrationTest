using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using Prefabs;
using Utils;
using Scenes;

namespace Network
{
    [Serializable]
    public sealed class SceneManagerTable : SerializableDictionary<int, GameObject> { }

    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private HostMigrationHandler _hostMigrationHandler;
        [SerializeField] private SceneManagerTable _sceneManagerTable;

        public static NetworkManager Instance;
        public static NetworkRunner Runner;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            if (Runner == null) Runner = this.gameObject.GetComponent<NetworkRunner>();
            else Destroy(this.gameObject);
        }

        public async Task<bool> JoinSession(NetworkRunner runner, StartGameArgs args)
        {
            var result = await runner.StartGame(args);

            if (result.Ok)
            {
                if (runner.IsServer)
                {
                    Debug.Log("Session Role : Host");
                    SessionInfoCache.Instance = new SessionInfoCache();
                }
                else Debug.Log("Session Role : Client");

                return true;
            }
            else
            {
                Debug.LogError($"Error : {result.ShutdownReason}");
                return false;
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                int token = new Guid(runner.GetPlayerConnectionToken(player)).GetHashCode();
                var playerInfoList = FindObjectsOfType<PlayerInfo>();
                var resumePlayer = playerInfoList.FirstOrDefault(player => player.connectionToken == token);

                if (resumePlayer != null)
                {
                    resumePlayer.Object.AssignInputAuthority(player);
                    var playerObj = resumePlayer.gameObject.GetComponent<NetworkObject>();

                    if (playerObj.InputAuthority.PlayerId == runner.LocalPlayer.PlayerId)
                    {
                        playerObj.GetComponent<ObjectToken>().token = "HOST";
                    }

                    SessionInfoCache.Instance.playerList.Add(player, playerObj);
                }
                else
                {
                    var playerObj = SessionInfoCache.Instance.sceneManager.SpawnPlayer(runner, player);
                    SessionInfoCache.Instance.playerList.Add(player, playerObj);
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                if (SessionInfoCache.Instance.playerList.TryGetValue(player, out NetworkObject networkObj))
                {
                    runner.Despawn(networkObj);
                    SessionInfoCache.Instance.playerList.Remove(player);
                }
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            var hostMigrationHandler = Instantiate(_hostMigrationHandler);
            hostMigrationHandler.RebootRunner(runner, hostMigrationToken);
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (runner.IsServer && !runner.IsResume)
            {
                if (_sceneManagerTable.TryGetValue(SceneManager.GetActiveScene().buildIndex, out var sceneManagerPrefab))
                {
                    var sceneManager = runner.Spawn(sceneManagerPrefab, onBeforeSpawned : (_, obj) =>
                    {
                        obj.GetComponent<ObjectToken>().token = Guid.NewGuid().ToString();
                    });

                    SessionInfoCache.Instance.SetSceneManager(sceneManager.GetComponent<ISceneManager>());
                }
            }
        }

        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    }
}
