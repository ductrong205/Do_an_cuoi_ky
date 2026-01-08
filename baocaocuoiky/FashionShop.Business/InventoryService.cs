using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FashionShop.Data;

namespace FashionShop.Business
{
    public class InventoryService
    {
        private readonly FashionShopDb _context;

        public InventoryService(FashionShopDb context)
        {
            _context = context;
        }

        public List<InventoryDto> GetAll()
        {
            var data = (from i in _context.Inventories
                        join v in _context.ProductVariants on i.VariantId equals v.VariantId
                        join p in _context.Products on v.ProductId equals p.ProductId
                        select new InventoryDto
                        {
                            Sku = p.Sku,
                            ProductName = p.ProductName,
                            Size = v.Size,
                            Color = v.Color,
                            Location = i.Location,
                            Quantity = i.Quantity,
                            UpdatedAt = i.UpdatedAt
                        })
                        .ToList();

            return data;
        }

        // Lấy tổng số sản phẩm (distinct SKU)
        public int GetTotalProducts()
        {
            return _context.Products.Count();
        }

        // Lấy tổng giá trị tồn kho (giá * số lượng)
        public decimal GetTotalStockValue()
        {
            var total = (from i in _context.Inventories
                         join v in _context.ProductVariants on i.VariantId equals v.VariantId
                         join p in _context.Products on v.ProductId equals p.ProductId
                         select (decimal?)(p.Price * i.Quantity))
                        .Sum();

            return total ?? 0;
        }

        // Lấy số SKU có cảnh báo thấp (<= 10)
        public int GetLowAlertCount()
        {
            return _context.Inventories.Count(i => i.Quantity > 0 && i.Quantity <= 10);
        }

        // Lấy số đơn đang nhập (giả định: đơn hàng có status=1 - "Đang giao" hoặc "Chờ xử lý")
        public int GetInboundOrders()
        {
            return _context.Orders.Count(o => o.Status == 0 || o.Status == 1); // Chờ xử lý + Đang giao
        }
    }

    public class InventoryDto
    {
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Location { get; set; }
        public int Quantity { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper: tên sản phẩm đầy đủ (Sku + Size + Color)
        public string FullProductName => $"{ProductName} - {Size} {Color}";
    }
}