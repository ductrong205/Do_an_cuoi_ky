using System.Collections.Generic;
using System.Linq;
using FashionShop.Data;

namespace FashionShop.Business
{
    public class CustomerService
    {
        private readonly FashionShopDb _context;

        public CustomerService(FashionShopDb context)
        {
            _context = context;
        }

        public List<Customer> GetAll()
        {
            return _context.Customers
                .OrderByDescending(c => c.JoinDate)
                .ToList();
        }

        public List<Order> GetOrdersByCustomerId(string customerCode)
        {
            var customerId = _context.Customers
                .Where(c => c.CustomerCode == customerCode)
                .Select(c => c.CustomerId)
                .FirstOrDefault();

            if (customerId == 0) return new List<Order>();

            return _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        // Hàm hỗ trợ: lấy nội dung đơn hàng (tạm thời lấy tên sản phẩm đầu tiên)
        public string GetOrderContent(int orderId)
        {
            var items = _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Join(_context.Products, oi => oi.ProductId, p => p.ProductId, (oi, p) => p.ProductName)
                .Take(2)
                .ToList();

            if (items.Count == 0) return "Không có sản phẩm";
            return string.Join(", ", items);
        }

    }
}