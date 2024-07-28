using Scenes;
using System.Collections.Generic;
using Fusion;

namespace Utils
{
    public class SessionInfoCache
    {
        public static SessionInfoCache Instance;

        public ISceneManager sceneManager { get; private set; }
        public Dictionary<PlayerRef, NetworkObject> playerList = new Dictionary<PlayerRef, NetworkObject>();

        public void SetSceneManager(ISceneManager manager)
        {
            sceneManager = manager;
        }
    }
}