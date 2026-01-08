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
                var u = db.Users
                          .Include("Role")
                          .FirstOrDefault(x =>
                              x.UserName == username &&
                              x.IsActive);

                if (u == null) return null;

                // tạm so sánh plain text
                if (u.PasswordHash != password) return null;

                return u;
            }
        }

    }
}
