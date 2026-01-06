CREATE DATABASE FashionShopDb;
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
USE FashionShopDb;
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
