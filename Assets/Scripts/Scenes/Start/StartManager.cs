using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Network;
using Constant;
using Utils;

namespace Scenes.Start
{
    public class StartManager : MonoBehaviour
    {
        [Header("Scene Objects")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [Header("Prefabs")]
        [SerializeField] private NetworkRunner _runnerPrefab;

        private void Awake()
        {
            //���[�U�[���
            if (UserInfo.Instance == null)
            {
                UserInfo.Instance = new UserInfo(
                    UnityEngine.Random.Range(0, 100),
                    "hogehoge");
            }
        }

        /// <summary>
        /// �z�X�g�Q��
        /// </summary>
        public async void PushHostButton()
        {
            //�{�^�����b�N
            ButtonLockAll();

            //Runner�N��
            var runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            //StartGame����
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host, //�Z�b�V��������
                Scene = SceneRef.FromIndex((int)SceneName.InGame), //���[�h����Q�[���V�[��
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), //�V�[���}�l�[�W���[
                SessionName = "test", //�Z�b�V������
                PlayerCount = 4, //�ő�l��
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //�Z�b�V�����Q��
            var result = await NetworkManager.Instance.JoinSession(runner, false, args);

            //�{�^�����b�N����
            if (!result) ButtonReleaseAll();
        }

        /// <summary>
        /// �N���C�A���g�Q��
        /// </summary>
        public async void PushClientButton()
        {
            //�{�^�����b�N
            ButtonLockAll();

            //Runner�N��
            var runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            //Runner����
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Client, //�Z�b�V��������
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), //�V�[���}�l�[�W���[
                SessionName = "test", //�Z�b�V������
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //�Z�b�V�����Q��
            var result = await NetworkManager.Instance.JoinSession(runner, false, args);

            //�{�^�����b�N����
            if (!result) ButtonReleaseAll();
        }

        /// <summary>
        /// �S�Ẵ{�^�������b�N
        /// </summary>
        private void ButtonLockAll()
        {
            _hostButton.interactable = false;
            _clientButton.interactable = false;
        }

        /// <summary>
        /// �S�Ẵ{�^��������
        /// </summary>
        private void ButtonReleaseAll()
        {
            _hostButton.interactable = true;
            _clientButton.interactable = true;
        }
    }
}
