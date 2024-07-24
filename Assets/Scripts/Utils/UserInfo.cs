namespace Utils
{
    public class UserInfo
    {
        public static UserInfo Instance;

        public int userId;
        public string userName;

        public UserInfo(int id, string name)
        {
            userId = id;
            userName = name;
        }
    }
}