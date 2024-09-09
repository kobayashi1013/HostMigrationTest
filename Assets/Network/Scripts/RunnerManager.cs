using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using UniRx;

namespace Network
{
    [Serializable]
    public sealed class SceneManagerTable : SerializableDictionary<int, GameObject> { }

    public class RunnerManager : MonoBehaviour
    {
        [SerializeField] private bool _hostMigration = false;
        [SerializeField] private SceneManagerTable _sceneManagerTable;

        public static NetworkRunner Runner;
        public static RunnerManager Instance;
        public IObservable<Unit> OnPlayerJoinedCall { get { return _playerJoinedCallSubject; } }

        public Subject<Unit> _playerJoinedCallSubject = new Subject<Unit>();
        private Dictionary<PlayerRef, NetworkObject> _playerList;

        private void Awake()
        {
            if (Runner == null) Runner = GetComponent<NetworkRunner>();
            else Destroy(this.gameObject);

            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);
        }

        public async Task<bool> JoinSession(StartGameArgs args)
        {
            var result = await Runner.StartGame(args);

            if (result.Ok)
            {
                if (Runner.IsServer)
                {
                    Debug.Log("Session Role : Host");
                    _playerList = new Dictionary<PlayerRef, NetworkObject>();
                }
                else
                {
                    Debug.Log("Session Role : Client");
                }

                return true;
            }
            else
            {
                Debug.LogError($"Error : {result.ShutdownReason}");
                return false;
            }
        }

        public NetworkObject PlayerSpawned(GameObject prefab, Vector3 position, Quaternion rotation, PlayerRef player)
        {
            if (!Runner.IsServer) return null;

            var playerObj = Runner.Spawn(prefab, position, rotation, player);
            return playerObj;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;

            if (_hostMigration)
            {
                _playerJoinedCallSubject.OnNext(Unit.Default);
            }
            else
            {
                _playerJoinedCallSubject.OnNext(Unit.Default);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("true");
            if (!runner.IsServer || runner.IsResume) return;

            if (_sceneManagerTable.TryGetValue(SceneManager.GetActiveScene().buildIndex, out var sceneManagerPrefab))
            {
                runner.Spawn(sceneManagerPrefab);
            }
        }

        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    }
}
