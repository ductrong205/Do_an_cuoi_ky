using System;
using System.Collections.Generic;
using System.Drawing;

namespace FashionShop.Shared
{
    public class DashboardSummaryDto
    {
        public List<OrderRowDto> RecentOrders { get; set; } = new List<OrderRowDto>();

        public class OrderRowDto
        {
            public string Code { get; set; }
            public string Customer { get; set; }
            public string DateText { get; set; }
            public decimal Total { get; set; }
            public string StatusText { get; set; }
            public Color StatusColor { get; set; }
        }
        public decimal TodayRevenue { get; set; }
        public int NewOrdersToday { get; set; }
        public int TotalStock { get; set; }
        public int NewCustomersToday { get; set; }
        public int TotalOrders { get; set; }

        public decimal RevenueChangePercent { get; set; }   // so với hôm qua
        public decimal OrdersChangePercent { get; set; }
        public decimal StockChangePercent { get; set; }
        public decimal CustomersChangePercent { get; set; }

        public List<RevenueDayDto> Revenue7Days { get; set; } = new List<RevenueDayDto>();
        public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
    }

    public class RevenueDayDto
    {
        public DateTime Day { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } // ← THÊM DÒNG NÀY
        public string Sku { get; set; }
        public int Sold { get; set; }
        public decimal Revenue { get; set; }

        public static TopProductDto Make(string productName, string sku, int sold, decimal revenue)
        {
            return new TopProductDto
            {
                ProductName = productName,
                Sku = sku,
                Sold = sold,
                Revenue = revenue
            };
        }
    }
}
