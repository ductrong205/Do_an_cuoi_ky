using FashionShop.Data;
using System;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public static class AppContext
    {
        private static FashionShopDb _dbContext;

        public static FashionShopDb DbContext
        {
            get
            {
                if (_dbContext == null || _dbContext.Database.Connection.State == System.Data.ConnectionState.Closed)
                {
                    _dbContext = new FashionShopDb();
                }
                return _dbContext;
            }
        }

        public static void CloseDbContext()
        {
            _dbContext?.Dispose();
            _dbContext = null;
        }

        // Đóng DbContext khi ứng dụng đóng
        public static void RegisterAppExit()
        {
            Application.ApplicationExit += (s, e) => CloseDbContext();
        }
    }
}