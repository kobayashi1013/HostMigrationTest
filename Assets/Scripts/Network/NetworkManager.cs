using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using Constant;
using Utils;
using Prefabs;
using Interface;

namespace Network
{
    [Serializable]
    public sealed class SceneManagerTable : SerializableDictionary<int, GameObject> { }

    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public SceneManagerTable _sceneManagerTable; //シーンマネージャーテーブル
        [SerializeField] private HostMigrationHandler _hostMigrationHandlerPrefab; //ホストマイグレーション実行

        public static NetworkManager Instance;
        public static NetworkRunner Runner;

        private void Awake()
        {
            //インスタンス化
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            if (Runner == null) Runner = this.gameObject.GetComponent<NetworkRunner>();
            else Destroy(this.gameObject);
        }

        /// <summary>
        /// セッション参加
        /// </summary>
        /// <param name="runner"></param> //Runner
        /// <param name="isHostMigration"></param> //ホストマイグレーションか否か
        /// <param name="args"></param> //StartGameArgs引数
        /// <returns></returns>
        public async Task<bool> JoinSession(NetworkRunner runner, bool isHostMigration, StartGameArgs args)
        {
            //セッション参加
            var result = await runner.StartGame(args);

            if (result.Ok)
            {
                if (runner.IsServer)
                {
                    Debug.Log("Session Role : Host");
                    SessionInfoCache.Instance = new SessionInfoCache(isHostMigration); //ホストが持つセッション情報
                }
                else Debug.Log("Session Role : Client");

                return true;
            }
            else
            {
                Debug.LogError($"error : {result.ShutdownReason}");

                return false;
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            //ホスト権限
            if (runner.IsServer)
            {
                //ホストマイグレーション時の復元確認
                int token = new Guid(runner.GetPlayerConnectionToken(player)).GetHashCode();
                var playerInfoList = FindObjectsOfType<PlayerInfo>();
                var newPlayer = playerInfoList.FirstOrDefault(player => player.connectionToken == token);

                //プレイヤー権限付与
                if (newPlayer != null) //復元プレイヤー
                {
                    newPlayer.Object.AssignInputAuthority(player);
                    SessionInfoCache.Instance.playerList.Add(player, newPlayer.gameObject.GetComponent<NetworkObject>());
                }
                else //新規プレイヤー
                {
                    var sceneManager = SessionInfoCache.Instance.sceneManager;
                    var playerObj = sceneManager.SpawnPlayer(runner, player);
                    SessionInfoCache.Instance.playerList.Add(player, playerObj);
                }
            }
        }

        /// <summary>
        /// プレイヤーの退出
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="player"></param>
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            //ホスト権限
            if (runner.IsServer)
            {
                //プレイヤー削除
                if (SessionInfoCache.Instance.playerList.TryGetValue(player, out NetworkObject networkObj))
                {
                    var sceneManager = SessionInfoCache.Instance.sceneManager;
                    sceneManager.DespawnPlayer(runner, networkObj);
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

        /// <summary>
        /// ホストマイグレーション
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="hostMigrationToken"></param>
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            //ハンドラの準備
            var hostMigrationHandler = Instantiate(_hostMigrationHandlerPrefab);
            hostMigrationHandler.RebootRunner(runner, hostMigrationToken);
        }

        /// <summary>
        /// シーンロード完了
        /// </summary>
        /// <param name="runner"></param>
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            //ホスト権限
            if (runner.IsServer)
            {
                //ホストマイグレーション中はスポーンさせない
                if (SessionInfoCache.Instance.hostMigration) return;

                //シーンマネージャーのスポーン
                if (_sceneManagerTable.TryGetValue(SceneManager.GetActiveScene().buildIndex, out var sceneManagerPrefab))
                {
                    var sceneManager = runner.Spawn(sceneManagerPrefab);
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
