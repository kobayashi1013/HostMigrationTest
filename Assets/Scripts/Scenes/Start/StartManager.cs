using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Constant;
using Network;

namespace Scenes.Start
{
    public class StartManager : MonoBehaviour
    {
        [Header("SceneObjects")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [Header("Prefabs")]
        [SerializeField] private NetworkRunner _runnerPrefab;

        public async void OnHostButton()
        {
            ButtonLockAll();

            var runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host,
                Scene = SceneRef.FromIndex((int)SceneName.InLobby),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                SessionName = "test",
                PlayerCount = 4,
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await NetworkManager.Instance.JoinSession(runner, args);

            if (!result) ButtonReleaseAll();
        }

        public async void OnClientButton()
        {
            ButtonLockAll();

            var runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                SessionName = "test",
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await NetworkManager.Instance.JoinSession(runner, args);

            if (!result) ButtonReleaseAll();
        }

        private void ButtonLockAll()
        {
            _hostButton.interactable = false;
            _clientButton.interactable = false;
        }

        private void ButtonReleaseAll()
        {
            _hostButton.interactable = true;
            _clientButton.interactable = true;
        }
    }
}