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
            //ユーザー情報
            if (UserInfo.Instance == null)
            {
                UserInfo.Instance = new UserInfo(
                    UnityEngine.Random.Range(0, 100),
                    "hogehoge");
            }
        }

        /// <summary>
        /// ホスト参加
        /// </summary>
        public async void PushHostButton()
        {
            //ボタンロック
            ButtonLockAll();

            //Runner起動
            var runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            //StartGame引数
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host, //セッション権限
                Scene = SceneRef.FromIndex((int)SceneName.InGame), //ロードするゲームシーン
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), //シーンマネージャー
                SessionName = "test", //セッション名
                PlayerCount = 4, //最大人数
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //セッション参加
            var result = await NetworkManager.Instance.JoinSession(runner, false, args);

            //ボタンロック解除
            if (!result) ButtonReleaseAll();
        }

        /// <summary>
        /// クライアント参加
        /// </summary>
        public async void PushClientButton()
        {
            //ボタンロック
            ButtonLockAll();

            //Runner起動
            var runner = Instantiate(_runnerPrefab);
            runner.ProvideInput = true;
            DontDestroyOnLoad(runner);

            //Runner引数
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Client, //セッション権限
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), //シーンマネージャー
                SessionName = "test", //セッション名
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //セッション参加
            var result = await NetworkManager.Instance.JoinSession(runner, false, args);

            //ボタンロック解除
            if (!result) ButtonReleaseAll();
        }

        /// <summary>
        /// 全てのボタンをロック
        /// </summary>
        private void ButtonLockAll()
        {
            _hostButton.interactable = false;
            _clientButton.interactable = false;
        }

        /// <summary>
        /// 全てのボタンを解除
        /// </summary>
        private void ButtonReleaseAll()
        {
            _hostButton.interactable = true;
            _clientButton.interactable = true;
        }
    }
}
