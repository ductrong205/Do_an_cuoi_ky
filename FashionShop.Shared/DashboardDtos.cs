using System;
using System.Collections.Generic;

namespace FashionShop.Shared
{
    public class DashboardSummaryDto
    {
        public decimal TodayRevenue { get; set; }
        public int NewOrdersToday { get; set; }
        public int TotalStock { get; set; }
        public int NewCustomersToday { get; set; }

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
        public string ProductName { get; set; }
        public string VariantText { get; set; } // "Đen - Size XL"
        public int SoldQty { get; set; }
        public string ImagePath { get; set; }
    }
}
