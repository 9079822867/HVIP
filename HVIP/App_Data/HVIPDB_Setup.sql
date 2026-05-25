-- ============================================================
--  HVIP Homeopathic — Database Setup Script
--  Run this once on HVIPDB to create all required tables
-- ============================================================

USE HVIPDB;
GO

-- ── USERS ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id           INT            PRIMARY KEY IDENTITY(1,1),
        Name         NVARCHAR(100)  NOT NULL,
        Email        NVARCHAR(150)  NOT NULL,
        Phone        NVARCHAR(15)   NULL,
        PasswordHash NVARCHAR(64)   NOT NULL,
        Address      NVARCHAR(300)  NULL,
        City         NVARCHAR(100)  NULL,
        State        NVARCHAR(100)  NULL,
        Pincode      NVARCHAR(10)   NULL,
        RegisteredOn DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
    PRINT 'Table [Users] created.';
END
ELSE
    PRINT 'Table [Users] already exists.';
GO

-- ── ORDERS ─────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Id            INT            PRIMARY KEY IDENTITY(1,1),
        OrderNumber   NVARCHAR(30)   NOT NULL,
        UserId        INT            NULL,          -- NULL = guest checkout
        CustomerName  NVARCHAR(100)  NOT NULL,
        Email         NVARCHAR(150)  NOT NULL,
        Phone         NVARCHAR(15)   NOT NULL,
        Address       NVARCHAR(300)  NOT NULL,
        City          NVARCHAR(100)  NOT NULL,
        State         NVARCHAR(100)  NOT NULL,
        Pincode       NVARCHAR(10)   NOT NULL,
        PaymentMethod NVARCHAR(50)   NOT NULL DEFAULT 'COD',
        SubTotal      DECIMAL(10,2)  NOT NULL DEFAULT 0,
        Shipping      DECIMAL(10,2)  NOT NULL DEFAULT 0,
        GrandTotal    DECIMAL(10,2)  NOT NULL DEFAULT 0,
        Status        NVARCHAR(50)   NOT NULL DEFAULT 'Confirmed',
        OrderDate     DATETIME       NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Table [Orders] created.';
END
ELSE
    PRINT 'Table [Orders] already exists.';
GO

-- ── ORDER ITEMS ────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
BEGIN
    CREATE TABLE OrderItems (
        Id            INT            PRIMARY KEY IDENTITY(1,1),
        OrderId       INT            NOT NULL,
        ProductId     INT            NOT NULL,
        ProductName   NVARCHAR(200)  NOT NULL,
        ProductSize   NVARCHAR(50)   NULL,
        CategoryIcon  NVARCHAR(100)  NULL,
        CategoryColor NVARCHAR(20)   NULL,
        UnitPrice     DECIMAL(10,2)  NOT NULL,
        Quantity      INT            NOT NULL DEFAULT 1,
        Total         DECIMAL(10,2)  NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId)
            REFERENCES Orders(Id) ON DELETE CASCADE
    );
    PRINT 'Table [OrderItems] created.';
END
ELSE
    PRINT 'Table [OrderItems] already exists.';
GO

-- ── CONTACT MESSAGES ───────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContactMessages')
BEGIN
    CREATE TABLE ContactMessages (
        Id          INT           PRIMARY KEY IDENTITY(1,1),
        Name        NVARCHAR(100) NOT NULL,
        Email       NVARCHAR(150) NOT NULL,
        Phone       NVARCHAR(15)  NULL,
        Subject     NVARCHAR(100) NULL,
        Message     NVARCHAR(MAX) NOT NULL,
        ReceivedOn  DATETIME      NOT NULL DEFAULT GETDATE(),
        IsRead      BIT           NOT NULL DEFAULT 0
    );
    PRINT 'Table [ContactMessages] created.';
END
ELSE
    PRINT 'Table [ContactMessages] already exists.';
GO

-- ── DEMO USER (password = Demo@123) ───────────────────────
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'demo@hvip.com')
BEGIN
    INSERT INTO Users (Name, Email, Phone, PasswordHash, Address, City, State, Pincode, RegisteredOn)
    VALUES (
        'Demo User',
        'demo@hvip.com',
        '9876543210',
        -- SHA256( "HVIP2024_SECURE" + "Demo@123" + "HVIP2024_SECURE" )
        '2e77f190f9e4e3822824d45086e9ead0fabc7b9b91bdaed29041c24e2ca0028d',
        '15, Green Park',
        'New Delhi',
        'Delhi',
        '110016'
    );
    PRINT 'Demo user inserted.';
END
GO

PRINT 'HVIPDB setup complete.';
GO
