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
        public DashboardSummaryDto GetSummary()
        {
            using (var db = DbFactory.Create())
            {
                var dto = new DashboardSummaryDto();

                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);
                DateTime yesterday = today.AddDays(-1);

                // ===== DOANH THU HÔM NAY =====
                dto.TodayRevenue = GetRevenueByRange(db, today, tomorrow);
                var yRevenue = GetRevenueByRange(db, yesterday, today);
                dto.RevenueChangePercent = CalcChangePercent(yRevenue, dto.TodayRevenue);

                // ===== ĐƠN HÀNG MỚI =====
                dto.NewOrdersToday = db.Orders.Count(o => o.OrderDate >= today && o.OrderDate < tomorrow);
                var yOrders = db.Orders.Count(o => o.OrderDate >= yesterday && o.OrderDate < today);
                dto.OrdersChangePercent = CalcChangePercent(yOrders, dto.NewOrdersToday);

                // ===== TỒN KHO (theo Products.Stock) =====
                dto.TotalStock = db.Products.Select(p => (int?)p.Stock).DefaultIfEmpty(0).Sum() ?? 0;
                dto.StockChangePercent = 0; // tạm để 0, khi có StockMovement sẽ tính sau

                // ===== KHÁCH HÀNG MỚI =====
                dto.NewCustomersToday = db.Customers.Count(c => c.CreatedAt >= today && c.CreatedAt < tomorrow);
                var yCustomers = db.Customers.Count(c => c.CreatedAt >= yesterday && c.CreatedAt < today);
                dto.CustomersChangePercent = CalcChangePercent(yCustomers, dto.NewCustomersToday);

                // ===== BIỂU ĐỒ 7 NGÀY =====
                dto.Revenue7Days = GetRevenue7Days(db);

                // ===== TOP SẢN PHẨM BÁN CHẠY 7 NGÀY (theo ProductId) =====
                dto.TopProducts = GetTopProducts7Days(db, 4);

                return dto;
            }
        }

        private decimal GetRevenueByRange(FashionShopDb db, DateTime dateFrom, DateTime dateTo)
        {
            var q = from o in db.Orders
                    join i in db.OrderItems on o.OrderId equals i.OrderId
                    where o.OrderDate >= dateFrom
                       && o.OrderDate < dateTo
                       && o.Status != 3
                    select (decimal?)i.UnitPrice * i.Quantity;

            return q.DefaultIfEmpty(0).Sum() ?? 0;
        }

        private List<RevenueDayDto> GetRevenue7Days(FashionShopDb db)
        {
            var start = DateTime.Today.AddDays(-6);
            var end = DateTime.Today.AddDays(1);

            // Group theo ngày (EF6 dùng DbFunctions.TruncateTime)
            var data = (from o in db.Orders
                        join i in db.OrderItems on o.OrderId equals i.OrderId
                        where (o.OrderDate >= start && o.OrderDate < end) && o.Status != 3
                        group new { i, o } by DbFunctions.TruncateTime(o.OrderDate) into g
                        select new
                        {
                            Day = g.Key.Value,
                            Revenue = g.Sum(x => (decimal?)x.i.UnitPrice * x.i.Quantity) ?? 0
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

        private List<TopProductDto> GetTopProducts7Days(FashionShopDb db, int take)
        {
            var start = DateTime.Today.AddDays(-6);
            var end = DateTime.Today.AddDays(1);

            var top = (from o in db.Orders
                       join i in db.OrderItems on o.OrderId equals i.OrderId
                       where (o.OrderDate >= start && o.OrderDate < end) && o.Status != 3
                       group i by i.ProductId into g
                       orderby g.Sum(x => x.Quantity) descending
                       select new { ProductId = g.Key, Sold = g.Sum(x => x.Quantity) })
                      .Take(take)
                      .ToList();

            var ids = top.Select(x => x.ProductId).ToList();

            var products = db.Products
                .Where(p => ids.Contains(p.ProductId))
                .Select(p => new { p.ProductId, p.ProductName, p.ImagePath })
                .ToList();

            var result = new List<TopProductDto>();
            foreach (var t in top)
            {
                var p = products.FirstOrDefault(x => x.ProductId == t.ProductId);
                if (p == null) continue;

                result.Add(new TopProductDto
                {
                    ProductName = p.ProductName,
                    VariantText = "",          // schema bạn chưa đưa size/color vào OrderItem => để trống
                    SoldQty = t.Sold,
                    ImagePath = p.ImagePath
                });
            }

            return result;
        }

        private static decimal CalcChangePercent(decimal prev, decimal cur)
        {
            if (prev <= 0 && cur > 0) return 100;
            if (prev <= 0 && cur <= 0) return 0;
            return Math.Round(((cur - prev) / prev) * 100m, 1);
        }

        private static decimal CalcChangePercent(int prev, int cur)
            => CalcChangePercent((decimal)prev, (decimal)cur);
    }
}
