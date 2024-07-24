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
        /// Runner�̍č\�z
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="hostMigrationToken"></param>
        public async void RebootRunner(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            //�Z�b�V�����g�[�N���̋L�^
            var connectionTokenBytes = runner.GetPlayerConnectionToken(runner.LocalPlayer);

            //��Runner�̒�~
            await runner.Shutdown(true, ShutdownReason.HostMigration);
            NetworkManager.Instance = null;
            NetworkManager.Runner = null;

            //�VRunner�̋N��
            runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            //StartGame����
            var args = new StartGameArgs
            {
                Scene = SceneRef.FromIndex((int)SceneName.InGame), //���[�h����Q�[���V�[��
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = HostMigrationResume,
                ConnectionToken = connectionTokenBytes
            };

            //�Z�b�V�����Q��
            var result = await NetworkManager.Instance.JoinSession(runner, true, args);

            //�n���h���̏I��
            Destroy(this.gameObject);
        }

        /// <summary>
        /// �I�u�W�F�N�g����
        /// �V�����Z�b�V�����̃z�X�g�݂̂��ʂ�
        /// </summary>
        /// <param name="runner"></param>
        private void HostMigrationResume(NetworkRunner runner)
        {
            //�I�u�W�F�N�g�̕���
            foreach (var resumeObj in runner.GetResumeSnapshotNetworkObjects())
            {
                //�I�u�W�F�N�g�̕���
                if (resumeObj.TryGetComponent<PlayerInfo>(out var playerInfo)) //�v���C���[�I�u�W�F�N�g�̕���
                {
                    //���z�X�g�I�u�W�F�N�g�̏��O
                    if (!playerInfo.snapshot)
                    {
                        Debug.Log("Not Resume Object");
                        continue;
                    }

                    //���W
                    var position = playerInfo.position;
                    var rotation = playerInfo.rotation;

                    //�v���C���[�I�u�W�F�N�g�X�|�[��
                    runner.Spawn(resumeObj, position, rotation, onBeforeSpawned: (_, newObj) =>
                    {
                        Debug.Log("Spawn Player Migration");
                        newObj.CopyStateFrom(resumeObj); //��Ԃ̃R�s�[

                        if (newObj.TryGetComponent<PlayerInfo>(out var playerInfo))
                        {
                            if (resumeObj.InputAuthority.PlayerId == runner.LocalPlayer.PlayerId) playerInfo.SetSnapshot(false);
                            else playerInfo.SetSnapshot(true);
                        }
                    });
                }
                else if (resumeObj.TryGetComponent<ISceneManager>(out _)) //�V�[���I�u�W�F�N�g�̕���
                {
                    //�V�[���I�u�W�F�N�g�X�|�[��
                    runner.Spawn(resumeObj, onBeforeSpawned: (_, newObj) =>
                    {
                        newObj.CopyStateFrom(resumeObj); //��Ԃ̃R�s�[
                        SessionInfoCache.Instance.SetSceneManager(newObj.GetComponent<ISceneManager>());
                    });
                }
                else //���̑��I�u�W�F�N�g�̕���
                {
                    runner.Spawn(resumeObj, onBeforeSpawned: (_, newObj) =>
                    {
                        newObj.CopyStateFrom(resumeObj); //��Ԃ̃R�s�[
                    });
                }
            }

            //�z�X�g�}�C�O���[�V�����̏I��
            SessionInfoCache.Instance.SetHostMigration(false);
        }
    }
}
