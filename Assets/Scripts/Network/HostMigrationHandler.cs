using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Prefabs;
using Constant;
using Utils;
using Interface;

namespace Network
{
    public class HostMigrationHandler : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _runnerPrefab;

        /// <summary>
        /// Runnerの再構築
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="hostMigrationToken"></param>
        public async void RebootRunner(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            //セッショントークンの記録
            var connectionTokenBytes = runner.GetPlayerConnectionToken(runner.LocalPlayer);

            //旧Runnerの停止
            await runner.Shutdown(true, ShutdownReason.HostMigration);
            NetworkManager.Instance = null;
            NetworkManager.Runner = null;

            //新Runnerの起動
            runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            //StartGame引数
            var args = new StartGameArgs
            {
                Scene = SceneRef.FromIndex((int)SceneName.InGame), //ロードするゲームシーン
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = HostMigrationResume,
                ConnectionToken = connectionTokenBytes
            };

            //セッション参加
            var result = await NetworkManager.Instance.JoinSession(runner, true, args);

            //ハンドラの終了
            Destroy(this.gameObject);
        }

        /// <summary>
        /// オブジェクト復元
        /// 新しいセッションのホストのみが通る
        /// </summary>
        /// <param name="runner"></param>
        private void HostMigrationResume(NetworkRunner runner)
        {
            //オブジェクトの復元
            foreach (var resumeObj in runner.GetResumeSnapshotNetworkObjects())
            {
                //オブジェクトの復元
                if (resumeObj.TryGetComponent<PlayerInfo>(out var playerInfo)) //プレイヤーオブジェクトの復元
                {
                    //旧ホストオブジェクトの除外
                    if (!playerInfo.snapshot)
                    {
                        Debug.Log("Not Resume Object");
                        continue;
                    }

                    //座標
                    var position = playerInfo.position;
                    var rotation = playerInfo.rotation;

                    //プレイヤーオブジェクトスポーン
                    runner.Spawn(resumeObj, position, rotation, onBeforeSpawned: (_, newObj) =>
                    {
                        Debug.Log("Spawn Player Migration");
                        newObj.CopyStateFrom(resumeObj); //状態のコピー

                        if (newObj.TryGetComponent<PlayerInfo>(out var playerInfo))
                        {
                            if (resumeObj.InputAuthority.PlayerId == runner.LocalPlayer.PlayerId) playerInfo.SetSnapshot(false);
                            else playerInfo.SetSnapshot(true);
                        }
                    });
                }
                else if (resumeObj.TryGetComponent<ISceneManager>(out _)) //シーンオブジェクトの復元
                {
                    //シーンオブジェクトスポーン
                    runner.Spawn(resumeObj, onBeforeSpawned: (_, newObj) =>
                    {
                        newObj.CopyStateFrom(resumeObj); //状態のコピー
                        SessionInfoCache.Instance.SetSceneManager(newObj.GetComponent<ISceneManager>());
                    });
                }
                else //その他オブジェクトの復元
                {
                    runner.Spawn(resumeObj, onBeforeSpawned: (_, newObj) =>
                    {
                        newObj.CopyStateFrom(resumeObj); //状態のコピー
                    });
                }
            }

            //ホストマイグレーションの終了
            SessionInfoCache.Instance.SetHostMigration(false);
        }
    }
}
