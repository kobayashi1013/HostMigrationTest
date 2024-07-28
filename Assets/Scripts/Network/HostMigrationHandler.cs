using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Scenes;
using Utils;

namespace Network
{
    public class HostMigrationHandler : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _runnerPrefab;
        private List<string> _resumeTokens = new List<string>();

        public async void RebootRunner(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            var connectionTokenBytes = runner.GetPlayerConnectionToken(runner.LocalPlayer);

            var tokens = FindObjectsOfType<ObjectToken>().Select(x => x.token);
            _resumeTokens = new List<string>(tokens);

            await runner.Shutdown(true, ShutdownReason.HostMigration);
            NetworkManager.Instance = null;
            NetworkManager.Runner = null;

            runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;

            var args = new StartGameArgs
            {
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = HostMigrationResume,
                ConnectionToken = connectionTokenBytes,
            };

            await NetworkManager.Instance.JoinSession(runner, args);

            Destroy(this.gameObject);
        }

        private void HostMigrationResume(NetworkRunner runner)
        {
            foreach (var resumeObj in runner.GetResumeSnapshotNetworkObjects())
            {
                var objectToken = resumeObj.GetComponent<ObjectToken>().token;
                var position = resumeObj.GetComponent<ObjectToken>().position;
                var rotation = resumeObj.GetComponent<ObjectToken>().rotation;

                if (objectToken == "HOST") continue;

                if (_resumeTokens.Exists(x => x == objectToken))
                {
                    if (resumeObj.TryGetComponent<ISceneManager>(out _))
                    {
                        runner.Spawn(resumeObj, position, rotation, null, (_, newObj) =>
                        {
                            newObj.CopyStateFrom(resumeObj);
                            SessionInfoCache.Instance.SetSceneManager(newObj.GetComponent<ISceneManager>());
                        });
                    }
                    else
                    {
                        runner.Spawn(resumeObj, position, rotation, null, (_, newObj) =>
                        {
                            newObj.CopyStateFrom(resumeObj);
                        });
                    }
                }
            }
        }
    }
}
