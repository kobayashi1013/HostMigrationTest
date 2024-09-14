using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

namespace Network.HostMigration
{
    public class HostMigrationHandler
    {
        public static HostMigrationHandler Instance;

        public async void RebootRunner(NetworkRunner prevRunner, NetworkRunner newRunner, HostMigrationToken hostMigrationToken)
        {
            //Runnerを新しいものに入れ替える
            await prevRunner.Shutdown(true, ShutdownReason.HostMigration);
            newRunner.ProvideInput = true;

            var args = new StartGameArgs
            {
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = newRunner.GetComponent<NetworkSceneManagerDefault>(),
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = HostMigrationResume,
            };

            //セッションを開始する
            await newRunner.StartGame(args);

            //ハンドラの終了
            Instance = null;
        }

        private void HostMigrationResume(NetworkRunner runner)
        {

        }
    }
}
