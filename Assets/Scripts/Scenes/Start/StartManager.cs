using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Network;
using Constant;

namespace Scenes.Start
{
    public class StartManager : MonoBehaviour
    {
        [Header("Scene Objects")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [Header("Prefabs")]
        [SerializeField] private NetworkRunner _runnerPrefab;

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

            //Runner����
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host, //�Z�b�V��������
                Scene = SceneRef.FromIndex((int)SceneName.InGame), //���[�h����Q�[���V�[��
                SceneManager = this.gameObject.GetComponent<NetworkSceneManagerDefault>(), //�V�[���}�l�[�W���[
                SessionName = "test", //�Z�b�V������
                PlayerCount = 2, //�ő�l��
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //�Z�b�V�����Q��
            var result = await NetworkManager.Instance.JoinSession(runner, SessionRole.Host, args);

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
                //Scene = SceneRef.FromIndex((int)SceneName.InGame), //���[�h����Q�[���V�[��
                SceneManager = this.gameObject.GetComponent<NetworkSceneManagerDefault>(), //�V�[���}�l�[�W���[
                SessionName = "test", //�Z�b�V������
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //�Z�b�V�����Q��
            var result = await NetworkManager.Instance.JoinSession(runner, SessionRole.Client, args);

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
