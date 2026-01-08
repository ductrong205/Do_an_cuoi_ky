using FashionShop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FashionShop.Business
{
    public class ReportService
    {
        private readonly FashionShopDb _context;

        public ReportService(FashionShopDb context)
        {
            _context = context;
        }

        public ReportStats GetStatistics(string range)
        {
            var now = DateTime.Now;
            DateTime startDate, endDate;

            switch (range)
            {
                case "Hôm nay":
                    startDate = now.Date;
                    endDate = now;
                    break;
                case "Tuần":
                    startDate = now.AddDays(-7).Date;
                    endDate = now;
                    break;
                case "Tháng":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = now;
                    break;
                default:
                    startDate = now.AddMonths(-1).Date;
                    endDate = now;
                    break;
            }

            var orders = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == 2) // Hoàn thành
                .ToList();

            decimal totalRevenue = orders.Sum(o => o.TotalAmount);
            int totalOrders = orders.Count;
            decimal avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Profit: tạm tính 20% doanh thu
            decimal profit = totalRevenue * 0.2m;

            return new ReportStats
            {
                RevenueText = FormatMoney(totalRevenue),
                ProfitText = FormatMoney(profit),
                Orders = totalOrders,
                AovText = FormatMoney(avgOrderValue),

                RevenueChangeText = "+12%",
                ProfitChangeText = "+5%",
                OrdersChangeText = "-2%",
                AovChangeText = "+8%",

                RevenueChangeColor = System.Drawing.Color.FromArgb(60, 200, 120),
                ProfitChangeColor = System.Drawing.Color.FromArgb(60, 200, 120),
                OrdersChangeColor = System.Drawing.Color.FromArgb(255, 80, 80),
                AovChangeColor = System.Drawing.Color.FromArgb(60, 200, 120)
            };
        }

        public List<DailyRevenueDto> GetRevenueChart(string range)
        {
            var now = DateTime.Now;
            DateTime startDate, endDate;

            switch (range)
            {
                case "Hôm nay":
                    startDate = now.Date;
                    endDate = now;
                    break;
                case "Tuần":
                    startDate = now.AddDays(-7).Date;
                    endDate = now;
                    break;
                case "Tháng":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = now;
                    break;
                default:
                    startDate = now.AddMonths(-1).Date;
                    endDate = now;
                    break;
            }

            var dailyRevenue = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == 2)
                .GroupBy(o => DbFunctions.TruncateTime(o.OrderDate)) // ← SỬA Ở ĐÂY
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key.Value, // ← Ghi nhớ: TruncateTime trả về nullable DateTime
                    Total = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return dailyRevenue;
        }

        public List<CategorySalesDto> GetCategoryDonut()
        {
            var categorySales = _context.OrderItems
                .Join(_context.Products, oi => oi.ProductId, p => p.ProductId, (oi, p) => new { oi.Quantity, p.CategoryId })
                .Join(_context.Category, x => x.CategoryId, c => c.CategoryId, (x, c) => new { x.Quantity, c.CategoryName })
                .GroupBy(x => x.CategoryName)
                .Select(g => new CategorySalesDto
                {
                    CategoryName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            return categorySales;
        }

        public List<TopProductDto> GetTopSellingProducts()
        {
            var topProducts = _context.OrderItems
                .Join(_context.Products, oi => oi.ProductId, p => p.ProductId, (oi, p) => new { oi.Quantity, p.ProductName, p.Sku, p.Price })
                .GroupBy(x => new { x.ProductName, x.Sku, x.Price })
                .Select(g => new TopProductDto
                {
                    Name = g.Key.ProductName,
                    Sku = g.Key.Sku,
                    Sold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            return topProducts;
        }

        private string FormatMoney(decimal amount)
        {
            var nfi = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.GetCultureInfo("vi-VN").NumberFormat.Clone();
            nfi.NumberGroupSeparator = ".";
            return amount.ToString("#,0", nfi) + "đ";
        }
    }

    // DTOs for data transfer (not UI models)
    public class ReportStats
    {
        public string RevenueText { get; set; }
        public string ProfitText { get; set; }
        public int Orders { get; set; }
        public string AovText { get; set; }
        public string RevenueChangeText { get; set; }
        public string ProfitChangeText { get; set; }
        public string OrdersChangeText { get; set; }
        public string AovChangeText { get; set; }
        public System.Drawing.Color RevenueChangeColor { get; set; }
        public System.Drawing.Color ProfitChangeColor { get; set; }
        public System.Drawing.Color OrdersChangeColor { get; set; }
        public System.Drawing.Color AovChangeColor { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }

    public class CategorySalesDto
    {
        public string CategoryName { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class TopProductDto
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public int Sold { get; set; }
        public decimal Revenue { get; set; }
    }
}