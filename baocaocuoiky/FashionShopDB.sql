﻿CREATE DATABASE FashionShopDb;
GO
USE FashionShopDb;
GO
-- Role & User
CREATE TABLE Roles(
  RoleId INT IDENTITY PRIMARY KEY,
  RoleName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Users(
  UserId INT IDENTITY PRIMARY KEY,
  FullName NVARCHAR(100) NOT NULL,
  Email NVARCHAR(100) NOT NULL UNIQUE,
  UserName NVARCHAR(50) NOT NULL UNIQUE,
  PasswordHash NVARCHAR(255) NOT NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

-- Category & Product
CREATE TABLE Categories(
  CategoryId INT IDENTITY PRIMARY KEY,
  CategoryName NVARCHAR(100) NOT NULL UNIQUE,
  IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Products(
  ProductId INT IDENTITY PRIMARY KEY,
  ProductName NVARCHAR(150) NOT NULL,
  Sku NVARCHAR(50) NOT NULL UNIQUE,
  CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId),
  Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0),
  Stock INT NOT NULL DEFAULT 0 CHECK (Stock >= 0),
  ImagePath NVARCHAR(255) NULL,
  Description NVARCHAR(500) NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
-- Customer
CREATE TABLE Customers(
  CustomerId INT IDENTITY PRIMARY KEY,
  FullName NVARCHAR(100) NOT NULL,
  Phone NVARCHAR(20) NULL,
  Email NVARCHAR(100) NULL,
  Address NVARCHAR(200) NULL,
  IsVip BIT NOT NULL DEFAULT 0,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

-- Order & OrderItem
CREATE TABLE Orders(
  OrderId INT IDENTITY PRIMARY KEY,
  OrderCode NVARCHAR(30) NOT NULL UNIQUE,
  CustomerId INT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
  OrderDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
  Status INT NOT NULL DEFAULT 0, -- 0:Chờ xử lý,1:Đang giao,2:Hoàn thành,3:Hủy
  DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0 CHECK (DiscountPercent>=0 AND DiscountPercent<=100),
  Note NVARCHAR(300) NULL
);

CREATE TABLE OrderItems(
  OrderItemId INT IDENTITY PRIMARY KEY,
  OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId) ON DELETE CASCADE,
  ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
  Quantity INT NOT NULL CHECK (Quantity>0),
  UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice>=0)
);


-- seed
INSERT INTO Roles(RoleName) VALUES (N'Admin'),(N'Staff');
INSERT INTO Categories(CategoryName) VALUES (N'Áo thun'),(N'Quần jean'),(N'Áo sơ mi'),(N'Phụ kiện');
GO

-- 1) Variants: Size/Màu theo sản phẩm
IF OBJECT_ID('dbo.ProductVariants', 'U') IS NULL
BEGIN
    CREATE TABLE ProductVariants(
      VariantId INT IDENTITY PRIMARY KEY,
      ProductId INT NOT NULL,
      Size NVARCHAR(20) NOT NULL,
      Color NVARCHAR(30) NOT NULL,
      Barcode NVARCHAR(50) NULL,
      IsActive BIT NOT NULL DEFAULT 1,
      CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

      CONSTRAINT FK_ProductVariants_Products
        FOREIGN KEY(ProductId) REFERENCES Products(ProductId),

      CONSTRAINT UQ_ProductVariants UNIQUE(ProductId, Size, Color)
    );
END
GO

-- 2) Inventory: tồn kho cho từng Variant
IF OBJECT_ID('dbo.Inventories', 'U') IS NULL
BEGIN
    CREATE TABLE Inventories(
      InventoryId INT IDENTITY PRIMARY KEY,
      VariantId INT NOT NULL UNIQUE,
      Quantity INT NOT NULL DEFAULT 0 CHECK (Quantity >= 0),
      MinAlert INT NOT NULL DEFAULT 5 CHECK (MinAlert >= 0),
      Location NVARCHAR(30) NULL, -- ví dụ: KỆ-A-02
      UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

      CONSTRAINT FK_Inventories_Variants
        FOREIGN KEY(VariantId) REFERENCES ProductVariants(VariantId) ON DELETE CASCADE
    );
END
GO

-- 3) Stock Movements: nhập/xuất/điều chỉnh kho (điểm cộng lớn)
IF OBJECT_ID('dbo.StockMovements', 'U') IS NULL
BEGIN
    CREATE TABLE StockMovements(
      MoveId INT IDENTITY PRIMARY KEY,
      VariantId INT NOT NULL,
      MoveType INT NOT NULL, -- 0:IN, 1:OUT, 2:ADJUST
      Quantity INT NOT NULL CHECK (Quantity > 0),
      RefCode NVARCHAR(30) NULL, -- mã đơn / mã phiếu
      Note NVARCHAR(200) NULL,
      CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

      CONSTRAINT FK_StockMovements_Variants
        FOREIGN KEY(VariantId) REFERENCES ProductVariants(VariantId)
    );
END
GO

-- Tạo variant mặc định nếu chưa có
INSERT INTO ProductVariants(ProductId, Size, Color, Barcode)
SELECT p.ProductId, N'FREE', N'DEFAULT', NULL
FROM Products p
WHERE NOT EXISTS (
    SELECT 1 FROM ProductVariants v WHERE v.ProductId = p.ProductId
);

-- Tạo inventory cho variant mặc định và đưa Stock vào Quantity
INSERT INTO Inventories(VariantId, Quantity, MinAlert, Location)
SELECT v.VariantId, p.Stock, 5, NULL
FROM ProductVariants v
JOIN Products p ON p.ProductId = v.ProductId
WHERE v.Size = N'FREE' AND v.Color = N'DEFAULT'
  AND NOT EXISTS (SELECT 1 FROM Inventories i WHERE i.VariantId = v.VariantId);

-- Orders: thêm CreatedBy + Total + UpdatedAt để dashboard/report
IF COL_LENGTH('dbo.Orders', 'CreatedByUserId') IS NULL
    ALTER TABLE Orders ADD CreatedByUserId INT NULL;
GO

IF COL_LENGTH('dbo.Orders', 'TotalAmount') IS NULL
    ALTER TABLE Orders ADD TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
GO

IF COL_LENGTH('dbo.Orders', 'UpdatedAt') IS NULL
    ALTER TABLE Orders ADD UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME();
GO

-- FK CreatedBy
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Orders_Users')
BEGIN
    ALTER TABLE Orders
    ADD CONSTRAINT FK_Orders_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId);
END
GO

-- Customers: thêm Code + JoinDate + Rank (giống UI khách hàng)
IF COL_LENGTH('dbo.Customers', 'CustomerCode') IS NULL
    ALTER TABLE Customers ADD CustomerCode NVARCHAR(20) NULL;
GO

IF COL_LENGTH('dbo.Customers', 'JoinDate') IS NULL
    ALTER TABLE Customers ADD JoinDate DATETIME2 NOT NULL DEFAULT SYSDATETIME();
GO

IF COL_LENGTH('dbo.Customers', 'Rank') IS NULL
    ALTER TABLE Customers ADD Rank NVARCHAR(30) NOT NULL DEFAULT N'Normal';
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'admin')
BEGIN
  INSERT INTO Users(FullName, Email, UserName, PasswordHash, RoleId, IsActive)
  VALUES (N'Quản trị viên', 'admin@gmail.com', 'admin', '123', 1, 1);
END
GO


-- 1. XÓA DỮ LIỆU CŨ (nếu cần)
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM Inventories;
DELETE FROM StockMovements;
DELETE FROM Customers;
DELETE FROM ProductVariants;
DELETE FROM Products;

-- 2. CHÈN 20 SẢN PHẨM MẪU
SET IDENTITY_INSERT Products ON;

INSERT INTO Products (ProductId, ProductName, Sku, CategoryId, Price, Stock, Description, IsActive)
VALUES
-- Áo thun (CategoryId = 1)
(1, N'Áo thun trơn cotton basic', 'AT001', 1, 199000, 50, N'Chất cotton 100%, form rộng, dễ phối', 1),
(2, N'Áo thun graphic print độc quyền', 'AT002', 1, 249000, 30, N'Họa tiết in nổi, giới hạn theo mùa', 1),
(3, N'Áo thun dài tay unisex', 'AT003', 1, 279000, 25, N'Phù hợp thu đông, giữ ấm tốt', 1),
(4, N'Áo thun oversize streetwear', 'AT004', 1, 299000, 40, N'Phong cách đường phố, form rộng', 1),
(5, N'Áo thun polo nam cao cấp', 'AT005', 1, 349000, 20, N'Cổ bẻ, chất vải pique cao cấp', 1),

-- Quần jean (CategoryId = 2)
(6, N'Quần jean skinny nam xanh đậm', 'QJ001', 2, 499000, 35, N'Chất denim co giãn, ôm dáng', 1),
(7, N'Quần jean ống rộng nữ', 'QJ002', 2, 549000, 28, N'Form rộng trendy, lưng cao', 1),
(8, N'Quần jean rách gối cá tính', 'QJ003', 2, 479000, 22, N'Thiết kế rách nhẹ, trẻ trung', 1),
(9, N'Quần jean straight fit unisex', 'QJ004', 2, 529000, 30, N'Form thẳng, phù hợp mọi giới tính', 1),
(10, N'Quần jean baggy vintage', 'QJ005', 2, 599000, 18, N'Phong cách vintage, rộng thoải mái', 1),

-- Áo sơ mi (CategoryId = 3)
(11, N'Áo sơ mi trắng basic nam', 'ASM001', 3, 329000, 45, N'100% cotton, dễ giặt ủi', 1),
(12, N'Áo sơ mi caro đỏ', 'ASM002', 3, 359000, 38, N'Họa tiết caro kinh điển, form regular', 1),
(13, N'Áo sơ mi nữ tay bồng', 'ASM003', 3, 399000, 32, N'Thiết kế nữ tính, cổ bẻ nhỏ', 1),
(14, N'Áo sơ mi linen mùa hè', 'ASM004', 3, 379000, 27, N'Chất linen mát, thấm hút tốt', 1),
(15, N'Áo sơ mi công sở cao cấp', 'ASM005', 3, 459000, 20, N'Màu xanh navy, vải nhập khẩu', 1),

-- Phụ kiện (CategoryId = 4)
(16, N'Mũ bucket unisex', 'PK001', 4, 149000, 60, N'Chất vải cotton, nhiều màu', 1),
(17, N'Túi tote canvas', 'PK002', 4, 179000, 50, N'In logo minimal, bền đẹp', 1),
(18, N'Vòng tay da nam', 'PK003', 4, 229000, 40, N'Da bò thật, khóa bạc', 1),
(19, N'Khăn quàng cổ mùa đông', 'PK004', 4, 199000, 35, N'Len mềm, giữ ấm tốt', 1),
(20, N'Vớ cotton họa tiết', 'PK005', 4, 89000, 100, N'Gói 3 đôi, thấm hút mồ hôi', 1);

SET IDENTITY_INSERT Products OFF;

-- 4. TẠO PRODUCTVARIANTS (FREE/DEFAULT) CHO MỖI SẢN PHẨM
INSERT INTO ProductVariants(ProductId, Size, Color, Barcode)
SELECT p.ProductId, N'FREE', N'DEFAULT', p.Sku
FROM Products p
WHERE NOT EXISTS (
    SELECT 1 FROM ProductVariants v WHERE v.ProductId = p.ProductId
);

-- 5. TẠO INVENTORIES TỪ STOCK CỦA PRODUCTS
INSERT INTO Inventories(VariantId, Quantity, MinAlert, Location)
SELECT v.VariantId, p.Stock, 5, N'KHO-' + CAST(p.ProductId % 5 + 1 AS NVARCHAR(10))
FROM ProductVariants v
JOIN Products p ON p.ProductId = v.ProductId
WHERE NOT EXISTS (SELECT 1 FROM Inventories i WHERE i.VariantId = v.VariantId);

-- 1. XÓA DỮ LIỆU ĐƠN HÀNG CŨ (nếu có)
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM Customers;

-- 2. CHÈN 5 KHÁCH HÀNG MẪU
INSERT INTO Customers (FullName, Phone, Email, Address, IsVip, CustomerCode, JoinDate, Rank)
VALUES
(N'Nguyễn Thị Mai', '0912345678', 'mai.nguyen@gmail.com', N'123 Đường Lê Lợi, Q.1, TP.HCM', 1, 'C001', '2026-01-01', N'Gold Member'),
(N'Trần Văn Hùng', '0988777666', 'hung.tran@gmail.com', N'456 Đường Nguyễn Trãi, Q.3, TP.HCM', 0, 'C002', '2026-01-02', N'Silver Member'),
(N'Lê Thị Lan', '0909000111', 'lan.le@gmail.com', N'789 Đường Trần Hưng Đạo, Q.5, TP.HCM', 0, 'C003', '2026-01-03', N'Normal'),
(N'Phạm Văn Minh', '0812345678', 'minh.pham@gmail.com', N'321 Đường Hai Bà Trưng, Q.1, TP.HCM', 1, 'C004', '2026-01-04', N'Gold Member'),
(N'Hồ Ngọc Diệp', '0901234567', 'diep.ho@gmail.com', N'654 Đường Võ Văn Kiệt, Q.4, TP.HCM', 0, 'C005', '2026-01-05', N'Normal');

-- 3. CHÈN 8 ĐƠN HÀNG MẪU
INSERT INTO Orders (OrderCode, CustomerId, OrderDate, Status, DiscountPercent, Note, CreatedByUserId, TotalAmount, UpdatedAt)
VALUES
('ORD-001', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C001'), '2026-01-01 10:30:00', 2, 0, N'Giao hàng nhanh', NULL, 0, GETDATE()),
('ORD-002', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C002'), '2026-01-02 14:15:00', 1, 5, N'Giao trước 17h', NULL, 0, GETDATE()),
('ORD-003', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C003'), '2026-01-03 09:00:00', 0, 0, N'Gói quà tặng', NULL, 0, GETDATE()),
('ORD-004', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C001'), '2026-01-04 16:45:00', 2, 10, N'Không cần hóa đơn', NULL, 0, GETDATE()),
('ORD-005', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C004'), '2026-01-05 11:20:00', 1, 0, N'Giao hàng tận nơi', NULL, 0, GETDATE()),
('ORD-006', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C005'), '2026-01-06 13:00:00', 0, 0, N'Ưu tiên giao sớm', NULL, 0, GETDATE()),
('ORD-007', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C002'), '2026-01-07 15:30:00', 2, 15, N'Tặng kèm túi tote', NULL, 0, GETDATE()),
('ORD-008', (SELECT CustomerId FROM Customers WHERE CustomerCode = 'C003'), '2026-01-08 08:45:00', 1, 0, N'Giao hàng theo giờ', NULL, 0, GETDATE());

-- 4. CHÈN CHI TIẾT ĐƠN HÀNG
-- ORD-001
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 2, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'AT001'
WHERE o.OrderCode = 'ORD-001';

-- ORD-002
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'QJ001'
WHERE o.OrderCode = 'ORD-002';

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'PK001'
WHERE o.OrderCode = 'ORD-002';

-- ORD-003
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'ASM001'
WHERE o.OrderCode = 'ORD-003';

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'AT002'
WHERE o.OrderCode = 'ORD-003';

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 2, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'PK005'
WHERE o.OrderCode = 'ORD-003';

-- ORD-004
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 3, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'QJ005'
WHERE o.OrderCode = 'ORD-004';

-- ORD-005
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'ASM002'
WHERE o.OrderCode = 'ORD-005';

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'AT003'
WHERE o.OrderCode = 'ORD-005';

-- ORD-006
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 2, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'PK003'
WHERE o.OrderCode = 'ORD-006';

-- ORD-007
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'AT004'
WHERE o.OrderCode = 'ORD-007';

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'QJ002'
WHERE o.OrderCode = 'ORD-007';

-- ORD-008
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
SELECT o.OrderId, p.ProductId, 1, p.Price
FROM Orders o JOIN Products p ON p.Sku = 'ASM003'
WHERE o.OrderCode = 'ORD-008';

-- 5. CẬP NHẬT TỔNG TIỀN CHO MỖI ĐƠN HÀNG
UPDATE Orders
SET TotalAmount = (
    SELECT ISNULL(SUM(oi.Quantity * oi.UnitPrice), 0)
    FROM OrderItems oi
    WHERE oi.OrderId = Orders.OrderId
);

