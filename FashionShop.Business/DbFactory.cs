using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FashionShop.Data;

namespace FashionShop.Business
{
    public static class DbFactory
    {
        public static FashionShopDb Create()
        {
            return new FashionShopDb();
        }
    }
}

