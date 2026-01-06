using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FashionShop.Data;

namespace FashionShop.Business
{
    public class ProductRowDto
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string ImagePath { get; set; }
    }

    public class ProductService
    {
        public List<ProductRowDto> GetAll(string keyword)
        {
            using (var db = DbFactory.Create())
            {
                var q =
                    from p in db.Products
                    join c in db.Category on p.CategoryId equals c.CategoryId
                    select new ProductRowDto
                    {
                        ProductId = p.ProductId,
                        Sku = p.Sku,
                        ProductName = p.ProductName,
                        CategoryName = c.CategoryName,
                        Price = p.Price,
                        Stock = p.Stock,
                        IsActive = p.IsActive,
                        ImagePath = p.ImagePath
                    };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim();
                    q = q.Where(x =>
                        x.ProductName.Contains(keyword) ||
                        x.Sku.Contains(keyword) ||
                        x.CategoryName.Contains(keyword));
                }

                return q.OrderBy(x => x.ProductName).ToList();
            }
        }

        public Product GetById(int id)
        {
            using (var db = DbFactory.Create())
            {
                return db.Products.FirstOrDefault(x => x.ProductId == id);
            }
        }

        public int Create(Product p)
        {
            using (var db = DbFactory.Create())
            {
                p.CreatedAt = DateTime.Now;
                db.Products.Add(p);
                db.SaveChanges();
                return p.ProductId;
            }
        }

        public void Update(Product p)
        {
            using (var db = DbFactory.Create())
            {
                db.Entry(p).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void DeleteHard(int id)
        {
            using (var db = DbFactory.Create())
            {
                var p = db.Products.FirstOrDefault(x => x.ProductId == id);
                if (p == null) return;
                db.Products.Remove(p);
                db.SaveChanges();
            }
        }

        public void Disable(int id)
        {
            using (var db = new FashionShopDb())
            {
                var p = db.Products.Find(id);
                if (p == null) return;

                p.IsActive = false;     // xóa mềm
                db.SaveChanges();       // ✅ BẮT BUỘC
            }
        }

    }
}
