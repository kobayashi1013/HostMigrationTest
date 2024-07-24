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
        public SceneManagerTable _sceneManagerTable; //�V�[���}�l�[�W���[�e�[�u��
        [SerializeField] private HostMigrationHandler _hostMigrationHandlerPrefab; //�z�X�g�}�C�O���[�V�������s

        public static NetworkManager Instance;
        public static NetworkRunner Runner;

        private void Awake()
        {
            //�C���X�^���X��
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            if (Runner == null) Runner = this.gameObject.GetComponent<NetworkRunner>();
            else Destroy(this.gameObject);
        }

        /// <summary>
        /// �Z�b�V�����Q��
        /// </summary>
        /// <param name="runner"></param> //Runner
        /// <param name="isHostMigration"></param> //�z�X�g�}�C�O���[�V�������ۂ�
        /// <param name="args"></param> //StartGameArgs����
        /// <returns></returns>
        public async Task<bool> JoinSession(NetworkRunner runner, bool isHostMigration, StartGameArgs args)
        {
            //�Z�b�V�����Q��
            var result = await runner.StartGame(args);

            if (result.Ok)
            {
                if (runner.IsServer)
                {
                    Debug.Log("Session Role : Host");
                    SessionInfoCache.Instance = new SessionInfoCache(isHostMigration); //�z�X�g�����Z�b�V�������
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
            //�z�X�g����
            if (runner.IsServer)
            {
                //�z�X�g�}�C�O���[�V�������̕����m�F
                int token = new Guid(runner.GetPlayerConnectionToken(player)).GetHashCode();
                var playerInfoList = FindObjectsOfType<PlayerInfo>();
                var newPlayer = playerInfoList.FirstOrDefault(player => player.connectionToken == token);

                //�v���C���[�����t�^
                if (newPlayer != null) //�����v���C���[
                {
                    newPlayer.Object.AssignInputAuthority(player);
                    SessionInfoCache.Instance.playerList.Add(player, newPlayer.gameObject.GetComponent<NetworkObject>());
                }
                else //�V�K�v���C���[
                {
                    var sceneManager = SessionInfoCache.Instance.sceneManager;
                    var playerObj = sceneManager.SpawnPlayer(runner, player);
                    SessionInfoCache.Instance.playerList.Add(player, playerObj);
                }
            }
        }

        /// <summary>
        /// �v���C���[�̑ޏo
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="player"></param>
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            //�z�X�g����
            if (runner.IsServer)
            {
                //�v���C���[�폜
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
        /// �z�X�g�}�C�O���[�V����
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="hostMigrationToken"></param>
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            //�n���h���̏���
            var hostMigrationHandler = Instantiate(_hostMigrationHandlerPrefab);
            hostMigrationHandler.RebootRunner(runner, hostMigrationToken);
        }

        /// <summary>
        /// �V�[�����[�h����
        /// </summary>
        /// <param name="runner"></param>
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            //�z�X�g����
            if (runner.IsServer)
            {
                //�z�X�g�}�C�O���[�V�������̓X�|�[�������Ȃ�
                if (SessionInfoCache.Instance.hostMigration) return;

                //�V�[���}�l�[�W���[�̃X�|�[��
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
