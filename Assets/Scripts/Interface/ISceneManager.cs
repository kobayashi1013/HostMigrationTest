using Fusion;
using System.Threading.Tasks;

namespace Interface
{
    public interface ISceneManager
    {
        public NetworkObject SpawnPlayer(NetworkRunner runner, PlayerRef player);
        public Task DespawnPlayer(NetworkRunner runner, NetworkObject playerObj);
    }
}