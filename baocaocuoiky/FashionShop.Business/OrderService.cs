using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FashionShop.Data;

namespace FashionShop.Business
{
    public class OrderService
    {
        private readonly FashionShopDb _context;

        public OrderService(FashionShopDb context)
        {
            _context = context;
        }

        // Tạo đơn hàng + OrderItems
        public int CreateOrder(Order order, List<OrderItem> items)
        {
            using (var db = new FashionShopDb()) // hoặc dùng _context nếu inject
            {
                // Kiểm tra từng sản phẩm có tồn tại không
                foreach (var item in items)
                {
                    var product = db.Products.Find(item.ProductId);
                    if (product == null)
                        throw new InvalidOperationException($"Sản phẩm ID={item.ProductId} không tồn tại!");
                }

                // Thêm đơn hàng
                db.Orders.Add(order);
                db.SaveChanges();

                // Thêm OrderItems
                foreach (var item in items)
                {
                    item.OrderId = order.OrderId; // Gắn OrderId
                    db.OrderItems.Add(item);
                }
                db.SaveChanges();

                return order.OrderId;
            }
        }

        // Lấy tất cả đơn hàng
        public List<Order> GetAll()
        {
            try
            {
                // ✅ DÙNG LEFT JOIN THỦ CÔNG để đảm bảo luôn lấy được đơn hàng
                return (from o in _context.Orders
                        join c in _context.Customers on o.CustomerId equals c.CustomerId into custs
                        from c in custs.DefaultIfEmpty() // ← LEFT JOIN
                        orderby o.OrderDate descending
                        select o)
                       .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Lỗi lấy danh sách đơn hàng", ex);
            }
        }

        // Lấy đơn hàng theo ID
        public Order GetById(int id)
        {
            using (var db = new FashionShopDb())
            {
                return db.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.OrderId == id);
            }
        }

        // Cập nhật trạng thái đơn hàng
        public void UpdateStatus(int orderId, int status)
        {
            using (var db = new FashionShopDb())
            {
                var order = db.Orders.Find(orderId);
                if (order != null)
                {
                    order.Status = status;
                    db.SaveChanges();
                }
            }
        }
    }
}