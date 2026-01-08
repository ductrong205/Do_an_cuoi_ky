namespace FashionShop.Shared
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
    }

    public static class AppSession
    {
        public static UserSession CurrentUser { get; private set; }
        public static void SetUser(UserSession u) => CurrentUser = u;
        public static void Clear() => CurrentUser = null;
    }
}
