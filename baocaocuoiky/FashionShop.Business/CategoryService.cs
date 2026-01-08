using FashionShop.Data;
using System.Collections.Generic;
using System.Linq;

namespace FashionShop.Business
{
    public class CategoryService
    {
        public List<Category> GetAll()
        {
            using (var db = DbFactory.Create())
            {
                return db.Category
                         .Where(x => x.IsActive)
                         .OrderBy(x => x.CategoryName)
                         .ToList();
            }
        }
    }
}
