using System.Collections.Generic;
using Fusion;
using Interface;

namespace Utils
{
    public class SessionInfoCache
    {
        public static SessionInfoCache Instance;

        public ISceneManager sceneManager { get; private set; } //���݂̃V�[���}�l�[�W���[
        public Dictionary<PlayerRef, NetworkObject> playerList = new Dictionary<PlayerRef, NetworkObject>(); //PlayerRef��Network�I�u�W�F�N�g�̕R�Â�
        public bool hostMigration { get; private set; } //�z�X�g�}�C�O���[�V���������ۂ�

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