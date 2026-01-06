using FashionShop.Data;
using System.Linq;

namespace FashionShop.Business
{
    public class AuthService
    {
        public User Login(string username, string password)
        {
            using (var db = DbFactory.Create())
            {
                var u = db.Users.FirstOrDefault(x => x.UserName == username && x.IsActive);
                if (u == null) return null;

                // Tạm thời so sánh plain cho dễ chạy
                // Sau này mình đổi sang BCrypt.Verify
                if (u.PasswordHash != password) return null;

                return u;
            }
        }
    }
}
