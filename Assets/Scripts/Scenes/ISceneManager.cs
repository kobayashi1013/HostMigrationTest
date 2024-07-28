using Fusion;

namespace Scenes
{
    public interface ISceneManager
    {
        public NetworkObject SpawnPlayer(NetworkRunner runner, PlayerRef player);
    }
}
