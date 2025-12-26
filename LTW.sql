CREATE DATABASE ShopThoiTrang
GO
USE ShopThoiTrang
GO



-- ===================================================
-- TẠO CÁC BẢNG CƠ BẢN
-- ===================================================

CREATE TABLE CategoryGroup (
    GroupId     INT IDENTITY PRIMARY KEY,
    GroupCode   VARCHAR(40) UNIQUE,
    GroupName   NVARCHAR(120),
    SortOrder   INT DEFAULT 0,
    IsActive    BIT DEFAULT 1
);

CREATE TABLE Category (
    CategoryId INT IDENTITY PRIMARY KEY,
    GroupId    INT NOT NULL,
    CatSlug    VARCHAR(60) UNIQUE,
    CatName    NVARCHAR(120),
    SortOrder  INT DEFAULT 0,
    IsActive   BIT DEFAULT 1,
    CONSTRAINT FK_Category_Group FOREIGN KEY(GroupId) REFERENCES dbo.CategoryGroup(GroupId)
);

CREATE TABLE [Product] (
    ProductId   INT IDENTITY PRIMARY KEY,
    SKU         VARCHAR(40) NOT NULL UNIQUE,
    ProductName NVARCHAR(180) NOT NULL,
    Slug        VARCHAR(120) NOT NULL UNIQUE,
    Description NVARCHAR(MAX) NULL,
    BasePrice   DECIMAL(12,0) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2 DEFAULT SYSUTCDATETIME(),
    CategoryId  INT NOT NULL,
    FOREIGN KEY(CategoryId) REFERENCES dbo.Category(CategoryId)
);

CREATE TABLE Size (
    SizeId INT IDENTITY PRIMARY KEY,
    SizeCode VARCHAR(10) NOT NULL UNIQUE,
    SortOrder INT NOT NULL
);

CREATE TABLE dbo.Color (
    ColorId INT IDENTITY PRIMARY KEY,
    ColorHex VARCHAR(30) UNIQUE,
    ColorName NVARCHAR(50)
);

CREATE TABLE ProductVariant (
    VariantId INT IDENTITY PRIMARY KEY,
    ProductId INT NOT NULL,
    SizeId    INT NOT NULL,
    ColorId   INT NOT NULL,
    Price     DECIMAL(12,0) NOT NULL,
    Stock     INT NOT NULL DEFAULT 0,
    SKU       VARCHAR(50) UNIQUE,
    CONSTRAINT FK_Variant_Product FOREIGN KEY(ProductId) REFERENCES dbo.Product(ProductId),
    CONSTRAINT FK_Variant_Size FOREIGN KEY(SizeId) REFERENCES dbo.Size(SizeId),
    CONSTRAINT FK_Variant_Color FOREIGN KEY(ColorId) REFERENCES dbo.Color(ColorId),
    CONSTRAINT UQ_ProductVariant UNIQUE(ProductId, SizeId, ColorId)
);

CREATE TABLE AppUser (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    Email           VARCHAR(120) NOT NULL UNIQUE,
    PasswordHash    VARCHAR(256) NOT NULL,
    FullName        NVARCHAR(120) NULL,
    Phone           VARCHAR(20) NULL,
    IsActive        BIT NOT NULL DEFAULT(1),
    CreatedAt       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Cart(
    CartId      INT IDENTITY(1,1) PRIMARY KEY,
    CartToken   VARCHAR(64) NOT NULL UNIQUE,
    UserId      INT NULL,
    CreatedAt   DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Cart_User FOREIGN KEY(UserId) REFERENCES dbo.AppUser(UserId)
);

CREATE TABLE CartItem (
    CartItemId INT IDENTITY PRIMARY KEY,
    CartId     INT NOT NULL,
    VariantId  INT NOT NULL,
    Quantity   INT CHECK (Quantity > 0),
    UnitPrice  DECIMAL(12,0) NOT NULL,
    FOREIGN KEY(CartId) REFERENCES dbo.Cart(CartId),
    FOREIGN KEY(VariantId) REFERENCES dbo.ProductVariant(VariantId)
);

CREATE TABLE CustomerAddress(
    AddressId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL,
    Line1       NVARCHAR(200) NOT NULL,
    Ward        NVARCHAR(100) NULL,
    District    NVARCHAR(100) NULL,
    Province    NVARCHAR(100) NULL,
    Note        NVARCHAR(200) NULL,
    CreatedAt   DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CustomerAddress_User FOREIGN KEY(UserId) REFERENCES AppUser(UserId)
);

CREATE TABLE [Order](
    OrderId         INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode       VARCHAR(30) NOT NULL UNIQUE,
    UserId          INT NULL,
    CustomerName    NVARCHAR(120) NOT NULL,
    Phone           VARCHAR(20) NOT NULL,
    AddressLine     NVARCHAR(220) NULL,
    MessageCard     NVARCHAR(240) NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT('PENDING'),
    TotalAmount     DECIMAL(12,0) NOT NULL DEFAULT(0),
    CreatedAt       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Order_User FOREIGN KEY(UserId) REFERENCES AppUser(UserId)
);

CREATE TABLE OrderItem (
    OrderItemId INT IDENTITY PRIMARY KEY,
    OrderId     INT NOT NULL,
    VariantId   INT NOT NULL,
    ProductName NVARCHAR(180),
    SizeName    NVARCHAR(50),
    ColorName   NVARCHAR(50),
    Quantity    INT,
    UnitPrice   DECIMAL(12,0),
    FOREIGN KEY(OrderId) REFERENCES [Order](OrderId)
);

CREATE TABLE [Role](
    Rid INT IDENTITY PRIMARY KEY,
    RName NVARCHAR(20)
);

CREATE TABLE UserRole (
    Rid INT,
    UserId INT,
    PRIMARY KEY (Rid, UserId),
    FOREIGN KEY (Rid) REFERENCES [Role](Rid),
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId)
);

CREATE TABLE ProductImage(
    ImageId     INT IDENTITY(1,1) PRIMARY KEY,
    ProductId   INT NOT NULL,
    ImageUrl    NVARCHAR(260) NOT NULL,
    IsPrimary   BIT NOT NULL DEFAULT(0),
    SortOrder   INT NOT NULL DEFAULT(0),
    CONSTRAINT FK_ProductImage_Product FOREIGN KEY(ProductId) REFERENCES dbo.Product(ProductId)
);

CREATE TABLE ProductReview (
    ReviewId        INT IDENTITY(1,1) PRIMARY KEY,
    ReviewCode      VARCHAR(30) NOT NULL UNIQUE,
    OrderItemId     INT NOT NULL,
	Rating          INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment         NVARCHAR(500) NULL,
    CreatedAt       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Review_OrderItem FOREIGN KEY(OrderItemId) REFERENCES OrderItem(OrderItemId)
);

CREATE TABLE ContactMessages (
    Id INT PRIMARY KEY IDENTITY(1,1), 
    FullName NVARCHAR(100) NOT NULL,  
    Email VARCHAR(100) NOT NULL,     
    MessageText NVARCHAR(MAX) NOT NULL, 
    CreatedAt DATETIME DEFAULT GETDATE(), 
    IsRead BIT DEFAULT 0              
);

CREATE TABLE Blogs (
    BlogId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(250) NOT NULL,        
    ImageThumb VARCHAR(250),           
    ShortDescription NVARCHAR(500),     
    FullContent NVARCHAR(MAX),          
    Author NVARCHAR(100),               
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1             
);
-- ===================================================
-- NHẬP DỮ LIỆU
-- ===================================================
INSERT Size(SizeCode, SortOrder) VALUES
('S',1),('M',2),('L',3),('XL',4),('2XL',5),('3XL',6);

-- Insert Color
INSERT dbo.Color (ColorHex, ColorName) VALUES
('#000000', N'Đen'),
('#FFFFFF', N'Trắng'),
('#FF0000', N'Đỏ'),
('#0000FF', N'Xanh dương'),
('#00FF00', N'Xanh lá'),
('#F5DEB3', N'Be');

-- Insert CategoryGroup
INSERT INTO CategoryGroup (GroupCode, GroupName, SortOrder, IsActive) VALUES
('ao', N'Áo', 0, 1),
('quan', N'Quần', 1, 1),
('phu-kien', N'Phụ Kiện', 2, 1);

-- Insert Category
INSERT INTO Category (GroupId, CatSlug, CatName, SortOrder, IsActive) VALUES
(1, 'ao-thun', N'Áo Thun', 0, 1),
(1, 'ao-so-mi', N'Áo Sơ Mi', 1, 1),
(1, 'ao-khoac', N'Áo Khoác', 2, 1),
(3, 'non', N'Nón', 0, 1),
(3, 'vo', N'Vớ', 1, 1),
(2, 'quan-short', N'Quần Short', 0, 1),
(2, 'quan-dai', N'Quần Dài', 1, 1),
(3, 'balo', N'Balo', 2, 1),       -- Sẽ nhận ID 8 (nếu tự động tăng tiếp theo)
(3, 'tui-deo', N'Túi đeo', 3, 1), -- Sẽ nhận ID 9
(3, 'giay', N'Giày', 4, 1);       -- Sẽ nhận ID 10

-- Insert Product (ĐÃ SỬA ĐÚNG CATEGORYID)
INSERT INTO Product (SKU, ProductName, Slug, Description, BasePrice, IsActive, CreatedAt, CategoryId)
VALUES
-- CategoryId = 1: Áo Thun
('P-001', N'Áo Thun Waffle Đen', 'ao-thun-waffle-den', N'Áo thun waffle thoáng mát, dễ phối đồ', 120650, 1, GETDATE(), 1),
('P-002', N'Áo Thun Pique Seventy Seven 013 Đen', 'ao-thun-pique-013', N'Áo thun pique cao cấp, form dáng basic', 149150, 1, GETDATE(), 1),
('P-003', N'Áo Thun Thể Thao Ultra Thin The Beginner 001 Đỏ Đậm', 'ao-thun-the-thao-001', N'Áo thun thể thao siêu mỏng, thấm hút tốt', 149150, 1, GETDATE(), 1),

-- CategoryId = 2: Áo Sơ Mi
('P-004', N'Áo Sơ Mi Trắng Tay Dài', 'ao-so-mi-trang-tay-dai', N'Áo sơ mi công sở lịch sự', 299000, 1, GETDATE(), 2),
('P-005', N'Áo Sơ Mi Xanh Nhạt', 'ao-so-mi-xanh-nhat', N'Áo sơ mi trẻ trung năng động', 299000, 1, GETDATE(), 2),
('P-006', N'Áo Sơ Mi Caro', 'ao-so-mi-caro', N'Áo sơ mi caro phong cách casual', 319000, 1, GETDATE(), 2),

-- CategoryId = 3: Áo Khoác
('P-007', N'Áo Khoác Kaki', 'ao-khoac-kaki', N'Áo khoác kaki phong cách Hàn Quốc', 499000, 1, GETDATE(), 3),
('P-008', N'Áo Khoác Jean', 'ao-khoac-jean', N'Áo khoác jean bền đẹp', 549000, 1, GETDATE(), 3),
('P-009', N'Áo Khoác Dù', 'ao-khoac-du', N'Áo khoác chống gió chống nước', 459000, 1, GETDATE(), 3),

-- CategoryId = 4: Nón
('P-010', N'Nón Lưỡi Trai Đen', 'non-luoi-trai-den', N'Nón lưỡi trai thời trang', 159000, 1, GETDATE(), 4),
('P-011', N'Nón Lưỡi Trai Trắng', 'non-luoi-trai-trang', N'Nón lưỡi trai basic', 159000, 1, GETDATE(), 4),
('P-012', N'Nón Bucket', 'non-bucket', N'Nón bucket phong cách Hàn Quốc', 179000, 1, GETDATE(), 4),

-- CategoryId = 5: Vớ
('P-013', N'Vớ Cổ Thấp', 'vo-co-thap', N'Vớ cotton thoáng khí', 49000, 1, GETDATE(), 5),
('P-014', N'Vớ Cổ Cao', 'vo-co-cao', N'Vớ thể thao co giãn tốt', 59000, 1, GETDATE(), 5),
('P-015', N'Vớ Trơn Basic', 'vo-tron-basic', N'Vớ trơn mặc hằng ngày', 45000, 1, GETDATE(), 5),

-- CategoryId = 6: Quần Short
('P-016', N'Quần Short Kaki', 'quan-short-kaki', N'Quần short kaki thoải mái', 259000, 1, GETDATE(), 6),
('P-017', N'Quần Short Jean', 'quan-short-jean', N'Quần short jean năng động', 279000, 1, GETDATE(), 6),
('P-018', N'Quần Short Thể Thao', 'quan-short-the-thao', N'Quần short vận động thoáng mát', 229000, 1, GETDATE(), 6),

-- CategoryId = 7: Quần Dài
('P-019', N'Quần Jean Slimfit', 'quan-jean-slimfit', N'Quần jean ôm vừa vặn', 399000, 1, GETDATE(), 7),
('P-020', N'Quần Tây Công Sở', 'quan-tay-cong-so', N'Quần tây lịch sự', 429000, 1, GETDATE(), 7),
('P-021', N'Quần Jogger', 'quan-jogger', N'Quần jogger phong cách thể thao', 349000, 1, GETDATE(), 7),
-- CategoryId = 8: Balo (Thêm 2 cái)
('P-022', N'Balo Thời Trang Collap Black', 'balo-collap-black', N'Balo chống nước nhẹ', 580000, 1, GETDATE(), 8),
('P-023', N'Balo Laptop Office', 'balo-laptop-office', N'Đựng vừa laptop 15.6 inch', 620000, 1, GETDATE(), 8),

-- CategoryId = 9: Túi đeo (Thêm 2 cái)
('P-024', N'Túi đeo chéo Canvas', 'tui-deo-cheo-canvas', N'Chất liệu vải canvas bền bỉ', 240000, 1, GETDATE(), 9),
('P-025', N'Túi đeo hông Đồ Thể Thao', 'tui-deo-hong-the-thao', N'Tiện lợi khi đi chạy bộ', 180000, 1, GETDATE(), 9),

-- CategoryId = 10: Giày (Thêm 2 cái)
('P-026', N'Giày Sneaker White Sport', 'giay-sneaker-white-sport', N'Đế cao su êm ái', 890000, 1, GETDATE(), 10),
('P-027', N'Giày Chạy Bộ Đồ Thể Thao Pro', 'giay-chay-bo-pro', N'Siêu nhẹ, thoát khí tốt', 1100000, 1, GETDATE(), 10);
GO
-------------------------------------<<<<<<<<<<<<<<<<<<<
-- Insert ProductImage
INSERT INTO ProductImage (ProductId, ImageUrl, IsPrimary, SortOrder)
VALUES
-- Áo Thun
(1, 'ao-thun1.jpg', 1, 0),(1, 'ao-thun-1-1.jpg', 0, 1), (1, 'ao-thun-1-2.jpg', 0, 2), (1, 'ao-thun-1-3.jpg', 0, 3),
(2, 'ao-thun2.jpg', 1, 0),(2, 'ao-thun-2-1.jpg', 0, 1), (2, 'ao-thun-2-2.jpg', 0, 2), (2, 'ao-thun-2-3.jpg', 0, 3),
(3, 'ao-thun3.jpg', 1, 0),(3, 'ao-thun-3-1.jpg', 0, 1), (3, 'ao-thun-3-2.jpg', 0, 2), (3, 'ao-thun-3-3.jpg', 0, 3),
-- Áo Sơ Mi
(4, 'so-mi1.jpg', 1, 0),(4, 'so-mi-4-1.jpg', 0, 1), (4, 'so-mi-4-2.jpg', 0, 2), (4, 'so-mi-4-3.jpg', 0, 3),
(5, 'so-mi2.jpg', 1, 0),(5, 'so-mi-5-1.jpg', 0, 1), (5, 'so-mi-5-2.jpg', 0, 2), (5, 'so-mi-5-3.jpg', 0, 3),
(6, 'so-mi3.jpg', 1, 0),(6, 'so-mi-6-1.jpg', 0, 1), (6, 'so-mi-6-2.jpg', 0, 2), (6, 'so-mi-6-3.jpg', 0, 3),
-- Áo Khoác
(7, 'khoac1.jpg', 1, 0),(7, 'khoac-7-1.jpg', 0, 1), (7, 'khoac-7-2.jpg', 0, 2), (7, 'khoac-7-3.jpg', 0, 3),
(8, 'khoac2.jpg', 1, 0),(8, 'khoac-8-1.jpg', 0, 1), (8, 'khoac-8-2.jpg', 0, 2), (8, 'khoac-8-3.jpg', 0, 3),
(9, 'khoac3.jpg', 1, 0),(9, 'khoac-9-1.jpg', 0, 1), (9, 'khoac-9-2.jpg', 0, 2), (9, 'khoac-9-3.jpg', 0, 3),
-- Nón
(10, 'non1.jpg', 1, 0),(10, 'non-10-1.jpg', 0, 1), (10, 'non-10-2.jpg', 0, 2), (10, 'non-10-3.jpg', 0, 3),
(11, 'non2.jpg', 1, 0),(11, 'non2-1.jpg', 0, 1), (11, 'non2-2.jpg', 0, 2), (11, 'non2-3.jpg', 0, 3),
(12, 'non3.jpg', 1, 0),(12, 'non3-1.jpg', 0, 1), (12, 'non3-2.jpg', 0, 2), (12, 'non3-3.jpg', 0, 3),

-- Vớ
(13, 'vo1.jpg', 1, 0),(13, 'vo1-1.jpg', 0, 1), (13, 'vo1-2.jpg', 0, 2), (13, 'vo1-3.jpg', 0, 3),
(14, 'vo2.jpg', 1, 0),(14, 'vo2-1.jpg', 0, 1), (14, 'vo2-2.jpg', 0, 2), (14, 'vo2-3.jpg', 0, 3),
(15, 'vo3.jpg', 1, 0),(15, 'vo3-1.jpg', 0, 1), (15, 'vo3-2.jpg', 0, 2), (15, 'vo3-3.jpg', 0, 3),
-- Quần Short
(16, 'quan-short1.jpg', 1, 0),(16, 'quan-short1-1.jpg', 0, 1), (16, 'quan-short1-2.jpg', 0, 2), (16, 'quan-short1-3.jpg', 0, 3),
(17, 'quan-short2.jpg', 1, 0),(17, 'quan-short2-1.jpg', 0, 4), (17, 'quan-short2-2.jpg', 0, 5),(17, 'quan-short2-2.jpg', 0, 6),
(18, 'quan-short3.jpg', 1, 0),(18, 'quan-short3-1.jpg', 0, 1), (18, 'quan-short3-2.jpg', 0, 2), (18, 'quan-short3-3.jpg', 0, 3),
-- Quần Dài
(19, 'quan-dai1.jpg', 1, 0),(19, 'quan-dai1-1.jpg', 0, 1), (19, 'quan-dai1-2.jpg', 0, 2), (19, 'quan-dai1-3.jpg', 0, 3),
(20, 'quan-dai2.jpg', 1, 0),(20, 'quan-dai2-1.jpg', 0, 1), (20, 'quan-dai2-2.jpg', 0, 2), (20, 'quan-dai2-3.jpg', 0, 3),
(21, 'quan-dai3.jpg', 1, 0),(21, 'quan-dai3-1.jpg', 0, 1), (21, 'quan-dai3-2.jpg', 0, 2), (21, 'quan-dai3-3.jpg', 0, 3),

-- P-022: Balo Thời Trang (ProductId 22)
(22, 'balo-thoi-trang-0.jpg', 1, 0), 
(22, 'balo-thoi-trang-1.jpg', 0, 1), 
(22, 'balo-thoi-trang-2.jpg', 0, 2), 
(22, 'balo-thoi-trang-3.jpg', 0, 3),

-- P-023: Balo Laptop (ProductId 23)
(23, 'balo-laptop-0.jpg', 1, 0), 
(23, 'balo-laptop-1.jpg', 0, 1), 
(23, 'balo-laptop-2.jpg', 0, 2), 
(23, 'balo-laptop-3.jpg', 0, 3),

-- P-024: Túi đeo chéo (ProductId 24)
(24, 'tui-deo-cheo-0.jpg', 1, 0), 
(24, 'tui-deo-cheo-1.jpg', 0, 1), 
(24, 'tui-deo-cheo-2.jpg', 0, 2), 
(24, 'tui-deo-cheo-3.jpg', 0, 3),

-- P-025: Túi đeo hông (ProductId 25)
(25, 'tui-deo-hong-0.jpg', 1, 0), 
(25, 'tui-deo-hong-1.jpg', 0, 1), 
(25, 'tui-deo-hong-2.jpg', 0, 2), 
(25, 'tui-deo-hong-3.jpg', 0, 3),

-- P-026: Giày Sneaker (ProductId 26)
(26, 'giay-sneaker-0.jpg', 1, 0), 
(26, 'giay-sneaker-1.jpg', 0, 1), 
(26, 'giay-sneaker-2.jpg', 0, 2), 
(26, 'giay-sneaker-3.jpg', 0, 3),

-- P-027: Giày Chạy Bộ (ProductId 27)
(27, 'giay-chay-bo-0.jpg', 1, 0), 
(27, 'giay-chay-bo-1.jpg', 0, 1), 
(27, 'giay-chay-bo-2.jpg', 0, 2), 
(27, 'giay-chay-bo-3.jpg', 0, 3),

(17, 'quan-short-desc1.jpg', 0, 1),
(17, 'quan-short-desc2.jpg', 0, 2),
(17, 'quan-short-desc3.jpg', 0, 3);

GO

-- Insert ProductVariant
INSERT INTO ProductVariant (ProductId, SizeId, ColorId, Price, Stock, SKU)
VALUES
-- ProductId 1-3: Áo Thun
(1, 1, 1, 120650, 50, 'P001-S-BLK'),
(1, 2, 1, 120650, 30, 'P001-M-BLK'),
(2, 1, 1, 149150, 40, 'P002-S-BLK'),
(2, 2, 1, 149150, 20, 'P002-M-BLK'),
(3, 1, 3, 149150, 50, 'P003-S-RED'),

-- ProductId 4-6: Áo Sơ Mi
(4, 1, 2, 299000, 40, 'P004-S-WHT'),
(4, 2, 2, 299000, 30, 'P004-M-WHT'),
(5, 1, 5, 299000, 35, 'P005-S-GRN'),
(6, 2, 1, 319000, 20, 'P006-M-BLK'),

-- ProductId 7-9: Áo Khoác
(7, 2, 1, 499000, 25, 'P007-M-BLK'),
(8, 2, 4, 549000, 20, 'P008-M-BLU'),
(9, 3, 6, 459000, 15, 'P009-L-BE'),

-- ProductId 10-12: Nón
(10, 1, 1, 159000, 100, 'P010-ONE-BLK'),
(11, 1, 2, 159000, 80, 'P011-ONE-WHT'),
(12, 1, 6, 179000, 60, 'P012-ONE-BE'),

-- ProductId 13-15: Vớ
(13, 1, 1, 49000, 200, 'P013-ONE-BLK'),
(14, 1, 4, 59000, 150, 'P014-ONE-BLU'),
(15, 1, 2, 45000, 180, 'P015-ONE-WHT'),

-- ProductId 16-18: Quần Short
(16, 2, 1, 259000, 50, 'P016-M-BLK'),
(16, 3, 1, 259000, 30, 'P016-L-BLK'),
(17, 2, 1, 279000, 40, 'P017-M-BLK'),
(17, 3, 1, 279000, 20, 'P017-L-BLK'),
(18, 1, 1, 229000, 50, 'P018-S-BLK'),

-- ProductId 19-21: Quần Dài
(19, 2, 1, 399000, 30, 'P019-M-BLK'),
(19, 3, 1, 399000, 20, 'P019-L-BLK'),
(20, 2, 1, 429000, 25, 'P020-M-BLK'),
(21, 3, 1, 349000, 15, 'P021-L-BLK'),

-- P-022: Balo Thời Trang Collap Black (ID 22) - Màu Đen (ID 1)
(22, 1, 1, 580000, 20, 'P022-S-BLK'),
(22, 2, 1, 580000, 15, 'P022-M-BLK'),

-- P-023: Balo Laptop Office (ID 23) - Màu Đen (ID 1)
(23, 2, 1, 620000, 10, 'P023-M-BLK'),
(23, 3, 1, 620000, 10, 'P023-L-BLK'),

-- P-024: Túi đeo chéo Canvas (ID 24) - Màu Be (ID 6)
(24, 1, 6, 240000, 30, 'P024-S-BE'),

-- P-025: Túi đeo hông Đồ Thể Thao (ID 25) - Màu Đen (ID 1)
(25, 1, 1, 180000, 25, 'P025-S-BLK'),

-- P-026: Giày Sneaker White Sport (ID 26) - Màu Trắng (ID 2)
(26, 1, 2, 890000, 12, 'P026-S-WHT'),
(26, 2, 2, 890000, 20, 'P026-M-WHT'),
(26, 3, 2, 890000, 15, 'P026-L-WHT'),

-- P-027: Giày Chạy Bộ Pro (ID 27) - Màu Xanh Dương (ID 4)
(27, 2, 4, 1100000, 10, 'P027-M-BLU'),
(27, 3, 4, 1100000, 8, 'P027-L-BLU');
GO

INSERT INTO AppUser (Email, PasswordHash, FullName, Phone, IsActive)
VALUES
('admin@gmail.vn', '123', N'Nguyễn Văn An', '0901234567', 1),
('khachhang@gmail.com', '123', N'Nguyễn Thị Bình', '0912345678', 1);

INSERT INTO Blogs (Title, ImageThumb, ShortDescription, FullContent, Author, CreatedAt, IsActive)
VALUES 
(N'Xu hướng thời trang Xuân Hè 2025', 'blog-1.jpg', N'Những gam màu pastel và chất liệu đũi đang lên ngôi trong mùa này.', N'Nội dung chi tiết về xu hướng Xuân Hè...', N'Admin', GETDATE(), 1),

(N'Cách phối đồ với áo Blazer cực chất', 'blog-2.jpg', N'Học cách mix blazer cho cả phong cách công sở và dạo phố.', N'Nội dung chi tiết về cách phối đồ Blazer...', N'Editor', GETDATE(), 1),

(N'Phụ kiện không thể thiếu của quý ông', 'blog-3.jpg', N'Đồng hồ và thắt lưng da - hai điểm nhấn quan trọng nhất.', N'Nội dung chi tiết về phụ kiện nam giới...', N'Admin', GETDATE(), 1),

(N'Review bộ sưu tập mới nhất tháng 12', 'blog-4.jpg', N'Cận cảnh những mẫu thiết kế vừa ra mắt tại sàn diễn Male Fashion.', N'Nội dung chi tiết review bộ sưu tập...', N'Fashionista', GETDATE(), 1),

(N'Mẹo bảo quản giày da luôn như mới', 'blog-5.jpg', N'Đừng để đôi giày đắt tiền của bạn bị hỏng chỉ vì thiếu kiến thức.', N'Nội dung chi tiết về bảo quản giày...', N'Admin', GETDATE(), 1),

(N'Phong cách Minimalism là gì?', 'blog-6.jpg', N'Tối giản không có nghĩa là đơn điệu. Hãy tìm hiểu cùng chúng tôi.', N'Nội dung chi tiết về phong cách tối giản...', N'Designer', GETDATE(), 1);

INSERT INTO CustomerAddress (UserId, Line1, Ward, District, Province, Note)
VALUES (1, N'Số 123 Đường Lê Lợi', N'Phường Bến Thành', N'Quận 1', N'TP. Hồ Chí Minh', N'Giao giờ hành chính'),
VALUES (2, N'Số 345 Đường Lê Lợi', N'Phường Bến Luck', N'Quận 10', N'TP. Hồ Chí Minh', N'Giao giờ nghỉ trưa');

insert into [role] values
(N'ADMIN'),
(N'CUSTOMER');

insert into userrole values
(1, 1),
(2,2);

