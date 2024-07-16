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

            //Runner引数
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host, //セッション権限
                Scene = SceneRef.FromIndex((int)SceneName.InGame), //ロードするゲームシーン
                SceneManager = this.gameObject.GetComponent<NetworkSceneManagerDefault>(), //シーンマネージャー
                SessionName = "test", //セッション名
                PlayerCount = 2, //最大人数
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //セッション参加
            var result = await NetworkManager.Instance.JoinSession(runner, SessionRole.Host, args);

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
                //Scene = SceneRef.FromIndex((int)SceneName.InGame), //ロードするゲームシーン
                SceneManager = this.gameObject.GetComponent<NetworkSceneManagerDefault>(), //シーンマネージャー
                SessionName = "test", //セッション名
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            //セッション参加
            var result = await NetworkManager.Instance.JoinSession(runner, SessionRole.Client, args);

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
