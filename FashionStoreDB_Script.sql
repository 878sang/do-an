-- ============================================================================
-- FASHION STORE DATABASE SCRIPT
-- Tạo database và tất cả các bảng cho hệ thống Fashion Store E-Commerce
-- ============================================================================

-- Tạo database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'FashionStoreDB')
BEGIN
    CREATE DATABASE FashionStoreDB;
END
GO

USE FashionStoreDB;
GO

-- ============================================================================
-- TẠO CÁC BẢNG
-- ============================================================================

-- 1. Bảng tb_Role (Vai trò)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Role]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Role] (
        [RoleId] INT IDENTITY(1,1) PRIMARY KEY,
        [RoleName] NVARCHAR(100) NULL,
        [Description] NVARCHAR(200) NULL
    );
END
GO

-- 2. Bảng tb_Account (Tài khoản Admin)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Account]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Account] (
        [AccountId] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(100) NULL,
        [Password] NVARCHAR(100) NULL,
        [FullName] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [RoleId] INT NULL,
        [LastLogin] DATETIME NULL,
        [IsActive] BIT NULL DEFAULT 1,
        CONSTRAINT [FK__tb_Accoun__RoleI__31B762FC] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tb_Role]([RoleId])
    );
END
GO

-- 3. Bảng tb_Customer (Khách hàng)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Customer]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Customer] (
        [CustomerId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [Username] NVARCHAR(50) NULL,
        [Password] NVARCHAR(100) NULL,
        [Birthday] DATE NULL,
        [Avatar] NVARCHAR(200) NULL,
        [Location] NVARCHAR(200) NULL,
        [LastLogin] DATETIME NULL,
        [IsActive] BIT NULL DEFAULT 1
    );
END
GO

-- 4. Bảng tb_ProductCategory (Danh mục sản phẩm)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_ProductCategory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_ProductCategory] (
        [CategoryProductId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NULL,
        [Alias] NVARCHAR(200) NULL,
        [Description] NTEXT NULL,
        [Icon] NVARCHAR(200) NULL,
        [Position] INT NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE()
    );
END
GO

-- 5. Bảng tb_Product (Sản phẩm)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Product]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Product] (
        [ProductId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NULL,
        [Alias] NVARCHAR(200) NULL,
        [CategoryProductId] INT NULL,
        [Description] NTEXT NULL,
        [Detail] NTEXT NULL,
        [Image] NVARCHAR(200) NULL,
        [Price] DECIMAL(18, 2) NULL,
        [PriceSale] DECIMAL(18, 2) NULL,
        [Quantity] INT NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [ModifiedDate] DATETIME NULL,
        [ModifiedBy] NVARCHAR(100) NULL,
        [IsNew] BIT NULL DEFAULT 0,
        [IsBestSeller] BIT NULL DEFAULT 0,
        [IsActive] BIT NULL DEFAULT 1,
        [Star] FLOAT NULL,
        CONSTRAINT [FK__tb_Produc__Categ__41EDCAC5] FOREIGN KEY ([CategoryProductId]) REFERENCES [dbo].[tb_ProductCategory]([CategoryProductId])
    );
END
GO

-- 6. Bảng tb_Color (Màu sắc)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Color]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Color] (
        [ColorId] INT IDENTITY(1,1) PRIMARY KEY,
        [ColorName] NVARCHAR(100) NULL,
        [ColorCode] NVARCHAR(50) NULL,
        [IsActive] BIT NULL DEFAULT 1
    );
END
GO

-- 7. Bảng tb_Size (Kích thước)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Size]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Size] (
        [SizeId] INT IDENTITY(1,1) PRIMARY KEY,
        [SizeName] NVARCHAR(50) NULL,
        [SizeOrder] INT NULL,
        [IsActive] BIT NULL DEFAULT 1
    );
END
GO

-- 8. Bảng tb_ProductVariant (Biến thể sản phẩm)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_ProductVariant]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_ProductVariant] (
        [VariantId] INT IDENTITY(1,1) PRIMARY KEY,
        [ProductId] INT NULL,
        [ColorId] INT NULL,
        [SizeId] INT NULL,
        [Image] NVARCHAR(200) NULL,
        [SKU] NVARCHAR(100) NULL,
        [Price] DECIMAL(18, 2) NULL,
        [PriceSale] DECIMAL(18, 2) NULL,
        [Quantity] INT NULL,
        [IsActive] BIT NULL DEFAULT 1,
        CONSTRAINT [FK__tb_Produc__Produ__44CA3770] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[tb_Product]([ProductId]),
        CONSTRAINT [FK__tb_Produc__Color__45BE5BA9] FOREIGN KEY ([ColorId]) REFERENCES [dbo].[tb_Color]([ColorId]),
        CONSTRAINT [FK__tb_Produc__SizeI__46B27FE2] FOREIGN KEY ([SizeId]) REFERENCES [dbo].[tb_Size]([SizeId])
    );
END
GO

-- 9. Bảng tb_OrderStatus (Trạng thái đơn hàng)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_OrderStatus]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_OrderStatus] (
        [OrderStatusId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(50) NULL,
        [Description] NVARCHAR(200) NULL
    );
END
GO

-- 10. Bảng tb_Order (Đơn hàng)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Order]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Order] (
        [OrderId] INT IDENTITY(1,1) PRIMARY KEY,
        [Code] NVARCHAR(50) NULL,
        [CustomerId] INT NULL,
        [ShippingAddress] NVARCHAR(200) NULL,
        [TotalAmount] DECIMAL(18, 2) NULL,
        [OrderStatusId] INT NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [ModifiedDate] DATETIME NULL,
        [Note] NTEXT NULL,
        [PaymentMethod] NVARCHAR(100) NULL,
        CONSTRAINT [FK__tb_Order__Custom__4A8310C6] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[tb_Customer]([CustomerId]),
        CONSTRAINT [FK__tb_Order__OrderS__4B7734FF] FOREIGN KEY ([OrderStatusId]) REFERENCES [dbo].[tb_OrderStatus]([OrderStatusId])
    );
END
GO

-- 11. Bảng tb_OrderDetail (Chi tiết đơn hàng)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_OrderDetail]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_OrderDetail] (
        [OrderId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [Price] DECIMAL(18, 2) NULL,
        [Quantity] INT NULL,
        PRIMARY KEY ([OrderId], [ProductId]),
        CONSTRAINT [FK__tb_OrderD__Order__4E53A1AA] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[tb_Order]([OrderId]) ON DELETE CASCADE,
        CONSTRAINT [FK__tb_OrderD__Produ__4F47C5E3] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[tb_Product]([ProductId]) ON DELETE CASCADE
    );
END
GO

-- 12. Bảng tb_ProductReview (Đánh giá sản phẩm)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_ProductReview]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_ProductReview] (
        [ProductReviewId] INT IDENTITY(1,1) PRIMARY KEY,
        [ProductId] INT NULL,
        [CustomerId] INT NULL,
        [Star] INT NULL,
        [Detail] NTEXT NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [IsActive] BIT NULL DEFAULT 1,
        CONSTRAINT [FK__tb_Produc__Produ__540C7B00] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[tb_Product]([ProductId]),
        CONSTRAINT [FK__tb_Produc__Custo__531856C7] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[tb_Customer]([CustomerId])
    );
END
GO

-- 13. Bảng tb_BlogCategory (Danh mục blog)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_BlogCategory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_BlogCategory] (
        [BlogCategoryId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NULL,
        [Alias] NVARCHAR(200) NULL,
        [Description] NTEXT NULL,
        [Image] NVARCHAR(200) NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE()
    );
END
GO

-- 14. Bảng tb_Blog (Blog)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Blog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Blog] (
        [BlogId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NULL,
        [Alias] NVARCHAR(200) NULL,
        [BlogCategoryId] INT NULL,
        [Description] NTEXT NULL,
        [Detail] NTEXT NULL,
        [Image] NVARCHAR(200) NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [ModifiedDate] DATETIME NULL,
        [ModifiedBy] NVARCHAR(100) NULL,
        [AccountId] INT NULL,
        [IsActive] BIT NULL DEFAULT 1,
        CONSTRAINT [FK__tb_Blog__BlogCat__57DD0BE4] FOREIGN KEY ([BlogCategoryId]) REFERENCES [dbo].[tb_BlogCategory]([BlogCategoryId]),
        CONSTRAINT [FK__tb_Blog__Account__58D1301D] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[tb_Account]([AccountId])
    );
END
GO

-- 15. Bảng tb_BlogComment (Bình luận blog)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_BlogComment]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_BlogComment] (
        [CommentId] INT IDENTITY(1,1) PRIMARY KEY,
        [BlogId] INT NULL,
        [CustomerId] INT NULL,
        [Detail] NTEXT NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [IsActive] BIT NULL DEFAULT 1,
        CONSTRAINT [FK__tb_BlogCo__BlogI__5CA1C101] FOREIGN KEY ([BlogId]) REFERENCES [dbo].[tb_Blog]([BlogId]),
        CONSTRAINT [FK__tb_BlogCo__Custo__5D95E53A] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[tb_Customer]([CustomerId])
    );
END
GO

-- 16. Bảng tb_Contact (Liên hệ)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Contact]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Contact] (
        [ContactId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [Message] NTEXT NULL,
        [IsRead] BIT NULL DEFAULT 0,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(50) NULL,
        [ModifiedDate] DATETIME NULL,
        [ModifiedBy] NVARCHAR(50) NULL
    );
END
GO

-- 17. Bảng tb_ChatMessage (Tin nhắn chat)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_ChatMessage]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_ChatMessage] (
        [MessageId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NULL,
        [GuestToken] NVARCHAR(100) NULL,
        [Sender] NVARCHAR(10) NOT NULL,
        [Message] NTEXT NOT NULL,
        [CreatedDate] DATETIME NULL,
        CONSTRAINT [FK_tb_ChatMessage_tb_Account] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tb_Account]([AccountId]) ON DELETE SET NULL
    );
    
    -- Tạo index cho bảng ChatMessage
    CREATE INDEX [IX_tb_ChatMessage_CreatedDate] ON [dbo].[tb_ChatMessage]([CreatedDate]);
    CREATE INDEX [IX_tb_ChatMessage_GuestToken] ON [dbo].[tb_ChatMessage]([GuestToken]);
    CREATE INDEX [IX_tb_ChatMessage_UserId] ON [dbo].[tb_ChatMessage]([UserId]);
END
GO

-- 18. Bảng tb_Menu (Menu)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_Menu]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_Menu] (
        [MenuId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NULL,
        [Alias] NVARCHAR(200) NULL,
        [Description] NTEXT NULL,
        [Position] INT NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [ModifiedDate] DATETIME NULL,
        [ModifiedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NULL DEFAULT 1
    );
END
GO

-- 19. Bảng tb_NewsCategory (Danh mục tin tức)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_NewsCategory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_NewsCategory] (
        [NewsCategoryId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NULL,
        [Alias] NVARCHAR(200) NULL,
        [Description] NTEXT NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE()
    );
END
GO

-- 20. Bảng tb_News (Tin tức)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_News]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_News] (
        [NewsId] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NULL,
        [Alias] NVARCHAR(200) NULL,
        [NewsCategoryId] INT NULL,
        [Description] NTEXT NULL,
        [Detail] NTEXT NULL,
        [Image] NVARCHAR(200) NULL,
        [CreatedDate] DATETIME NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [ModifiedDate] DATETIME NULL,
        [ModifiedBy] NVARCHAR(100) NULL,
        [IsActive] BIT NULL DEFAULT 1,
        CONSTRAINT [FK__tb_News__NewsCat__6166761E] FOREIGN KEY ([NewsCategoryId]) REFERENCES [dbo].[tb_NewsCategory]([NewsCategoryId])
    );
END
GO