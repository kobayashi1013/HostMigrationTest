using System.Collections.Generic;
using Fusion;
using Interface;

namespace Utils
{
    public class SessionInfoCache
    {
        public static SessionInfoCache Instance;

        public ISceneManager sceneManager { get; private set; } //現在のシーンマネージャー
        public Dictionary<PlayerRef, NetworkObject> playerList = new Dictionary<PlayerRef, NetworkObject>(); //PlayerRefとNetworkオブジェクトの紐づけ
        public bool hostMigration { get; private set; } //ホストマイグレーション中か否か

        public SessionInfoCache(bool isHostMigration)
        {
            hostMigration = isHostMigration;
        }

        public void SetSceneManager(ISceneManager value)
        {
            sceneManager = value;
        }

        public void SetHostMigration(bool value)
        {
            hostMigration = value;
        }
    }
}