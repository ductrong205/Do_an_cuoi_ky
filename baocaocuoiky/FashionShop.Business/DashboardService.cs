using FashionShop.Data;
using FashionShop.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FashionShop.Business
{
    public class DashboardService
    {
        private readonly FashionShopDb _context;

        public DashboardService(FashionShopDb context)
        {
            _context = context;
        }

        public DashboardSummaryDto GetSummary()
        {
            var dto = new DashboardSummaryDto();

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            DateTime yesterday = today.AddDays(-1);

            // ===== DOANH THU HÔM NAY (chỉ lấy đơn HOÀN THÀNH) =====
            dto.TodayRevenue = _context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.Status == 2)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            var yRevenue = _context.Orders
                .Where(o => o.OrderDate >= yesterday && o.OrderDate < today && o.Status == 2)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            dto.RevenueChangePercent = CalcChangePercent(yRevenue, dto.TodayRevenue);

            // ===== ĐƠN HÀNG MỚI (tất cả trạng thái) =====
            dto.NewOrdersToday = _context.Orders.Count(o => o.OrderDate >= today && o.OrderDate < tomorrow);
            var yOrders = _context.Orders.Count(o => o.OrderDate >= yesterday && o.OrderDate < today);
            dto.OrdersChangePercent = CalcChangePercent(yOrders, dto.NewOrdersToday);

            // ===== TỒN KHO (từ Inventories) =====
            dto.TotalStock = _context.Inventories.Sum(i => (int?)i.Quantity) ?? 0;
            // Tạm thời không có cách so sánh tuần trước => giữ 0
            dto.StockChangePercent = 0;

            // ===== KHÁCH HÀNG MỚI (hôm nay) =====
            dto.NewCustomersToday = _context.Customers.Count(c => c.JoinDate >= today && c.JoinDate < tomorrow);
            var yCustomers = _context.Customers.Count(c => c.JoinDate >= yesterday && c.JoinDate < today);
            dto.CustomersChangePercent = CalcChangePercent(yCustomers, dto.NewCustomersToday);

            // ===== BIỂU ĐỒ 7 NGÀY (chỉ đơn HOÀN THÀNH) =====
            dto.Revenue7Days = GetRevenue7Days();

            // ===== TOP SẢN PHẨM BÁN CHẠY 7 NGÀY =====
            dto.TopProducts = GetTopProducts7Days(4);

            // ===== ĐƠN HÀNG GẦN ĐÂY =====
            dto.RecentOrders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(4)
                .Select(o => new
                {
                    o.OrderCode,
                    CustomerName = o.Customer != null ? o.Customer.FullName : "Khách lẻ",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .AsEnumerable() // ← CHUYỂN SANG LINQ TO OBJECTS
                .Select(o =>
                {
                    string statusText;
                    switch (o.Status)
                    {
                        case 0:
                            statusText = "Chờ xử lý";
                            break;
                        case 1:
                            statusText = "Vận chuyển";
                            break;
                        case 2:
                            statusText = "Hoàn thành";
                            break;
                        case 3:
                            statusText = "Đã hủy";
                            break;
                        default:
                            statusText = "Không xác định";
                            break;
                    }

                    return new DashboardSummaryDto.OrderRowDto
                    {
                        Code = o.OrderCode,
                        Customer = o.CustomerName,
                        DateText = o.OrderDate.ToString("dd/MM/yyyy"),
                        Total = o.TotalAmount,
                        StatusText = statusText
                    };
                })
                .ToList();

            // ===== TỔNG SỐ ĐƠN HÀNG =====
            dto.TotalOrders = _context.Orders.Count();

            return dto;
        }

        private List<RevenueDayDto> GetRevenue7Days()
        {
            var start = DateTime.Today.AddDays(-6);
            var end = DateTime.Today.AddDays(1);

            // Nhóm theo ngày, chỉ lấy đơn HOÀN THÀNH
            var data = (from o in _context.Orders
                        where o.OrderDate >= start && o.OrderDate < end && o.Status == 2
                        group o by DbFunctions.TruncateTime(o.OrderDate) into g
                        select new
                        {
                            Day = g.Key.Value,
                            Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0
                        })
                       .ToList();

            var map = data.ToDictionary(x => x.Day.Date, x => x.Revenue);

            var result = new List<RevenueDayDto>();
            for (int k = 0; k < 7; k++)
            {
                var d = start.AddDays(k).Date;
                result.Add(new RevenueDayDto
                {
                    Day = d,
                    Revenue = map.ContainsKey(d) ? map[d] : 0
                });
            }
            return result;
        }

        public List<FashionShop.Shared.TopProductDto> GetTopProducts7Days(int take)
        {
            var today = DateTime.Now.Date;
            var sevenDaysAgo = today.AddDays(-7);

            var topProducts = _context.OrderItems
                .Join(_context.Products, oi => oi.ProductId, p => p.ProductId, (oi, p) => new { oi.Quantity, p.ProductName, p.Sku, p.Price })
                .Where(x => x.Quantity > 0)
                .GroupBy(x => new { x.ProductName, x.Sku, x.Price })
                .Select(g => new FashionShop.Shared.TopProductDto
                {
                    ProductName = g.Key.ProductName,
                    Sku = g.Key.Sku,
                    Sold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(take)
                .ToList();

            return topProducts;
        }

        private static decimal CalcChangePercent(decimal prev, decimal cur)
        {
            if (prev <= 0 && cur > 0) return 100;
            if (prev <= 0) return 0;
            return Math.Round(((cur - prev) / prev) * 100m, 1);
        }

        private static decimal CalcChangePercent(int prev, int cur)
        {
            return CalcChangePercent((decimal)prev, (decimal)cur);
        }
    }
}