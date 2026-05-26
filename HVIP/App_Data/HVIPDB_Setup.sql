-- ============================================================
--  HVIP Homeopathic — Full Database Setup Script v3
--  Run once on HVIPDB
-- ============================================================
USE HVIPDB;
GO

-- ── USERS ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Users')
BEGIN
    CREATE TABLE Users (
        Id           INT           PRIMARY KEY IDENTITY(1,1),
        Name         NVARCHAR(100) NOT NULL,
        Email        NVARCHAR(150) NOT NULL,
        Phone        NVARCHAR(15)  NULL,
        PasswordHash NVARCHAR(64)  NOT NULL,
        Address      NVARCHAR(300) NULL,
        City         NVARCHAR(100) NULL,
        State        NVARCHAR(100) NULL,
        Pincode      NVARCHAR(10)  NULL,
        IsAdmin      BIT           NOT NULL DEFAULT 0,
        RegisteredOn DATETIME      NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
    PRINT 'Table [Users] created.';
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Users') AND name='IsAdmin')
    BEGIN
        ALTER TABLE Users ADD IsAdmin BIT NOT NULL DEFAULT 0;
        PRINT '[Users] IsAdmin column added.';
    END
END
GO

-- ── CATEGORIES ─────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Categories')
BEGIN
    CREATE TABLE Categories (
        Id           INT           PRIMARY KEY IDENTITY(1,1),
        Name         NVARCHAR(100) NOT NULL,
        Slug         NVARCHAR(100) NOT NULL,
        Icon         NVARCHAR(100) NOT NULL DEFAULT 'fas fa-pills',
        Color        NVARCHAR(20)  NOT NULL DEFAULT '#1b5e20',
        Description  NVARCHAR(300) NULL,
        SortOrder    INT           NOT NULL DEFAULT 0,
        IsActive     BIT           NOT NULL DEFAULT 1,
        HomeNavbar   BIT           NOT NULL DEFAULT 1,
        FooterNavbar BIT           NOT NULL DEFAULT 1
    );
    PRINT 'Table [Categories] created.';
END
ELSE
BEGIN
    -- Add new columns to existing table if not present
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Categories') AND name='IsActive')
    BEGIN
        ALTER TABLE Categories ADD IsActive BIT NOT NULL DEFAULT 1;
        PRINT '[Categories] IsActive column added.';
    END
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Categories') AND name='HomeNavbar')
    BEGIN
        ALTER TABLE Categories ADD HomeNavbar BIT NOT NULL DEFAULT 1;
        PRINT '[Categories] HomeNavbar column added.';
    END
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Categories') AND name='FooterNavbar')
    BEGIN
        ALTER TABLE Categories ADD FooterNavbar BIT NOT NULL DEFAULT 1;
        PRINT '[Categories] FooterNavbar column added.';
    END
END
GO

-- ── PRODUCTS ───────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Products')
BEGIN
    CREATE TABLE Products (
        Id               INT           PRIMARY KEY IDENTITY(1,1),
        Name             NVARCHAR(200) NOT NULL,
        ShortDescription NVARCHAR(300) NULL,
        Description      NVARCHAR(MAX) NULL,
        Price            DECIMAL(10,2) NOT NULL,
        OriginalPrice    DECIMAL(10,2) NULL,
        ImageUrl         NVARCHAR(300) NULL,
        CategoryId       INT           NOT NULL,
        Brand            NVARCHAR(100) NOT NULL DEFAULT 'HVIP',
        Stock            INT           NOT NULL DEFAULT 100,
        IsFeatured       BIT           NOT NULL DEFAULT 0,
        IsBestseller     BIT           NOT NULL DEFAULT 0,
        IsNew            BIT           NOT NULL DEFAULT 0,
        Rating           DECIMAL(3,1)  NOT NULL DEFAULT 4.0,
        ReviewCount      INT           NOT NULL DEFAULT 0,
        Size             NVARCHAR(50)  NULL,
        IsActive         BIT           NOT NULL DEFAULT 1,
        CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
    );
    PRINT 'Table [Products] created.';
END
GO

-- ── BANNERS ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Banners')
BEGIN
    CREATE TABLE Banners (
        Id                INT           PRIMARY KEY IDENTITY(1,1),
        Title             NVARCHAR(200) NOT NULL,
        Subtitle          NVARCHAR(400) NULL,
        BadgeText         NVARCHAR(100) NULL,
        Icon              NVARCHAR(100) NULL DEFAULT 'fas fa-leaf',
        BgGradient        NVARCHAR(300) NOT NULL DEFAULT 'linear-gradient(135deg,#0d4a1e,#1a6e2e)',
        PrimaryLink       NVARCHAR(200) NULL,
        PrimaryLinkText   NVARCHAR(50)  NULL DEFAULT 'Shop Now',
        SecondaryLink     NVARCHAR(200) NULL,
        SecondaryLinkText NVARCHAR(50)  NULL,
        SortOrder         INT           NOT NULL DEFAULT 0,
        IsActive          BIT           NOT NULL DEFAULT 1
    );
    PRINT 'Table [Banners] created.';
END
GO

-- ── ORDERS ─────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Orders')
BEGIN
    CREATE TABLE Orders (
        Id            INT           PRIMARY KEY IDENTITY(1,1),
        OrderNumber   NVARCHAR(30)  NOT NULL,
        UserId        INT           NULL,
        CustomerName  NVARCHAR(100) NOT NULL,
        Email         NVARCHAR(150) NOT NULL,
        Phone         NVARCHAR(15)  NOT NULL,
        Address       NVARCHAR(300) NOT NULL,
        City          NVARCHAR(100) NOT NULL,
        State         NVARCHAR(100) NOT NULL,
        Pincode       NVARCHAR(10)  NOT NULL,
        PaymentMethod NVARCHAR(50)  NOT NULL DEFAULT 'COD',
        SubTotal      DECIMAL(10,2) NOT NULL DEFAULT 0,
        Shipping      DECIMAL(10,2) NOT NULL DEFAULT 0,
        GrandTotal    DECIMAL(10,2) NOT NULL DEFAULT 0,
        Status        NVARCHAR(50)  NOT NULL DEFAULT 'Confirmed',
        OrderDate     DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Table [Orders] created.';
END
GO

-- ── ORDER ITEMS ────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='OrderItems')
BEGIN
    CREATE TABLE OrderItems (
        Id            INT           PRIMARY KEY IDENTITY(1,1),
        OrderId       INT           NOT NULL,
        ProductId     INT           NOT NULL,
        ProductName   NVARCHAR(200) NOT NULL,
        ProductSize   NVARCHAR(50)  NULL,
        CategoryIcon  NVARCHAR(100) NULL,
        CategoryColor NVARCHAR(20)  NULL,
        UnitPrice     DECIMAL(10,2) NOT NULL,
        Quantity      INT           NOT NULL DEFAULT 1,
        Total         DECIMAL(10,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
    );
    PRINT 'Table [OrderItems] created.';
END
GO

-- ── CONTACT MESSAGES ───────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='ContactMessages')
BEGIN
    CREATE TABLE ContactMessages (
        Id         INT           PRIMARY KEY IDENTITY(1,1),
        Name       NVARCHAR(100) NOT NULL,
        Email      NVARCHAR(150) NOT NULL,
        Phone      NVARCHAR(15)  NULL,
        Subject    NVARCHAR(100) NULL,
        Message    NVARCHAR(MAX) NOT NULL,
        ReceivedOn DATETIME      NOT NULL DEFAULT GETDATE(),
        IsRead     BIT           NOT NULL DEFAULT 0
    );
    PRINT 'Table [ContactMessages] created.';
END
GO

-- ════════════════════════════════════════════════════════════
--  SEED DATA
-- ════════════════════════════════════════════════════════════

-- Categories
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    SET IDENTITY_INSERT Categories ON;
    INSERT INTO Categories (Id,Name,Slug,Icon,Color,Description,SortOrder) VALUES
    (1,'Dilutions','dilutions','fas fa-flask','#1b5e20','Classical homeopathic dilutions in various potencies',1),
    (2,'Mother Tinctures','mother-tinctures','fas fa-tint','#2e7d32','Plant-based mother tinctures (Q potency)',2),
    (3,'Biochemic Tablets','biochemic-tablets','fas fa-tablets','#00695c','12 tissue salts for biochemic therapy',3),
    (4,'Combination Remedies','combination','fas fa-capsules','#0277bd','Multi-ingredient drops for specific ailments',4),
    (5,'Ointments & Creams','ointments-creams','fas fa-fill-drip','#e65100','Topical homeopathic preparations',5),
    (6,'Hair Care','hair-care','fas fa-spa','#4e342e','Arnica-based hair care products',6),
    (7,'Skin Care','skin-care','fas fa-leaf','#880e4f','Natural skin care solutions',7),
    (8,'Drops & Syrups','drops-syrups','fas fa-prescription-bottle','#4a148c','Liquid homeopathic preparations',8);
    SET IDENTITY_INSERT Categories OFF;
    PRINT 'Categories seeded.';
END
GO

-- Products
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    SET IDENTITY_INSERT Products ON;
    INSERT INTO Products (Id,Name,ShortDescription,Description,Price,OriginalPrice,CategoryId,Brand,Stock,IsFeatured,IsBestseller,IsNew,Rating,ReviewCount,Size) VALUES
    (1,'Aconite Napellus 30C','For sudden fever, anxiety & shock','Indicated for sudden onset after exposure to cold wind. Effective for high fever, intense fear, restlessness and acute anxiety attacks.',85,100,1,'HVIP',150,1,1,0,4.5,128,'30ml'),
    (2,'Arnica Montana 30C','For bruises, injuries & muscle soreness','The most widely used homeopathic remedy for trauma, bruising and muscle soreness. Excellent after physical exertion, falls and surgery.',90,110,1,'HVIP',200,1,0,0,4.7,245,'30ml'),
    (3,'Belladonna 30C','For sudden high fever with redness','Prescribed for sudden violent fever with flushed face, throbbing headache, extreme sensitivity to light and dryness.',80,NULL,1,'HVIP',175,0,1,0,4.3,89,'30ml'),
    (4,'Calcarea Carb 200C','Constitutional remedy for slow metabolism','Deep constitutional remedy for obesity, profuse sweating of the head, delayed developmental milestones and weak bones in children.',95,115,1,'HVIP',120,1,0,0,4.6,156,'30ml'),
    (5,'Nux Vomica 30C','For digestive disorders & irritability','Indicated for digestive troubles from overeating, constipation, irritability and over-sensitivity to external stimuli.',80,NULL,1,'HVIP',190,0,1,0,4.4,112,'30ml'),
    (6,'Pulsatilla 30C','For changeable symptoms, mild temperament','A polychrest remedy for weeping tendency, changeable symptoms and conditions aggravated by warmth and improved in open air.',85,NULL,1,'HVIP',165,0,0,0,4.2,74,'30ml'),
    (7,'Rhus Tox 30C','For joint pain worse in cold damp weather','The leading remedy for joint stiffness and pain that is worse on first motion but improves with continued movement. Excellent for arthritis.',85,100,1,'HVIP',210,1,0,0,4.5,198,'30ml'),
    (8,'Calendula Officinalis Q','Wound healing & antiseptic','A powerful antiseptic and wound healer. Used internally and topically for cuts, wounds, ulcers and skin infections.',150,180,2,'HVIP',140,1,1,0,4.8,312,'30ml'),
    (9,'Echinacea Purpurea Q','Immunity booster, anti-infective','Widely used to boost immune system function and fight infections. Effective for recurring colds, flu and low-grade fevers.',180,NULL,2,'HVIP',125,0,0,0,4.5,87,'30ml'),
    (10,'Thuja Occidentalis Q','For warts, skin growths & polyps','Used for warts, condylomata, polyps and skin growths.',160,200,2,'HVIP',100,1,0,0,4.3,65,'30ml'),
    (11,'Berberis Vulgaris Q','Kidney stones & urinary complaints','Primary remedy for kidney stones, renal colic and urinary tract issues with radiating burning pain.',170,NULL,2,'HVIP',145,0,1,0,4.6,143,'30ml'),
    (12,'Ferrum Phosphoricum 6X','First stage of inflammation & fever','First aid biochemic salt for early stages of inflammation, fever and infections.',95,NULL,3,'HVIP',230,0,1,0,4.4,178,'25g'),
    (13,'Kali Phosphoricum 6X','Nerve tonic for mental fatigue','The nerve tissue salt for nervous exhaustion, mental fatigue, anxiety, depression and poor memory.',95,NULL,3,'HVIP',195,1,0,0,4.6,134,'25g'),
    (14,'Magnesia Phosphorica 6X','Cramps, spasms & neuralgic pain','The antispasmodic biochemic salt. Used for muscle cramps, menstrual cramps, colic and shooting neuralgic pains.',90,110,3,'HVIP',180,1,0,0,4.5,96,'25g'),
    (15,'Natrum Muriaticum 6X','Water balance & dry mucous membranes','Regulates body fluids. Used for headaches, cold sores, dry skin and hay fever.',95,NULL,3,'HVIP',210,0,0,0,4.3,67,'25g'),
    (16,'Calcarea Phosphorica 6X','Bone health & growing pains','Strengthens bones and teeth, aids calcium absorption. Used for growing pains and delayed teething.',90,NULL,3,'HVIP',165,0,0,0,4.2,54,'25g'),
    (17,'HVIP Febin Drops','For cold, cough and mild fever','A combination remedy for common cold, cough, runny nose and mild fever. Safe for adults and children above 2 years.',120,145,4,'HVIP',320,1,1,0,4.6,267,'30ml'),
    (18,'HVIP Stresswin Drops','For stress, anxiety & sleeplessness','Helps manage stress, anxiety, nervous tension and insomnia. A safe, non-habit forming natural remedy.',135,NULL,4,'HVIP',215,0,1,0,4.7,198,'30ml'),
    (19,'HVIP Diabonil Drops','Blood sugar management support','Supports healthy blood sugar levels as a complementary treatment for type 2 diabetes.',140,165,4,'HVIP',180,1,0,0,4.4,123,'30ml'),
    (20,'HVIP Calendula Ointment','Antiseptic cream for cuts & wounds','Natural antiseptic preparation promoting wound healing. Excellent for cuts, abrasions, burns.',130,155,5,'HVIP',145,0,1,0,4.7,289,'25g'),
    (21,'HVIP Arnica Ointment','For bruises, sprains & muscle pain','Provides relief from bruises, sprains, muscle soreness and joint pains.',125,NULL,5,'HVIP',190,1,0,0,4.5,176,'25g'),
    (22,'HVIP Clear Face Cream','Homeopathic acne & pimple control cream','With Calendula and Thuja, helps clear acne, pimples and blemishes.',195,220,5,'HVIP',250,1,1,0,4.8,412,'25g'),
    (23,'HVIP Arnica Hair Oil','Prevents hair fall, promotes growth','Arnica Montana Hair Oil reduces hair fall, promotes growth, prevents premature greying.',210,250,6,'HVIP',310,1,1,1,4.9,523,'100ml'),
    (24,'HVIP Hair Vitalizer','Scalp nourishment & dandruff control','Nourishes the scalp, strengthens hair roots and prevents dandruff.',285,320,6,'HVIP',195,0,1,0,4.6,287,'100ml'),
    (25,'HVIP Arnica Shampoo','Gentle cleansing, reduces hair fall','Arnica-enriched herbal shampoo for daily use. Reduces hair fall and promotes lustrous hair.',175,210,6,'HVIP',220,0,0,0,4.4,165,'200ml'),
    (26,'HVIP Aloe Vera Gel','Soothing moisturizer for all skin types','Aloe Vera Gel provides deep moisture, soothes sunburn and helps heal skin irritations.',175,NULL,7,'HVIP',275,1,0,1,4.5,163,'150ml'),
    (27,'HVIP Sunscreen SPF30','UV protection with natural ingredients','Broad-spectrum SPF30 sunscreen with Calendula and Berberis. Protects from UVA/UVB.',220,265,7,'HVIP',190,1,0,0,4.3,98,'50ml'),
    (28,'HVIP Ocudrops Eye Drops','For eye fatigue, redness & irritation','Provides relief from digital eye strain, redness, irritation and dryness.',115,NULL,8,'HVIP',155,0,0,0,4.4,112,'10ml'),
    (29,'HVIP Immunoboost Syrup','Builds immunity, prevents infections','Strengthens the immune system and improves overall vitality.',150,175,8,'HVIP',230,1,1,0,4.7,345,'100ml'),
    (30,'HVIP Digyton Drops','For indigestion, bloating & acidity','Treats indigestion, flatulence, bloating and gastric discomfort.',125,145,8,'HVIP',180,1,0,0,4.5,187,'30ml'),
    (31,'HVIP Sleepzyme Drops','Natural sleep aid, no side effects','Promotes restful sleep naturally with Coffea Cruda, Passiflora and Valeriana.',135,NULL,8,'HVIP',165,0,0,1,4.6,134,'30ml');
    SET IDENTITY_INSERT Products OFF;
    PRINT 'Products seeded (31 records).';
END
GO

-- Banners
IF NOT EXISTS (SELECT 1 FROM Banners)
BEGIN
    SET IDENTITY_INSERT Banners ON;
    INSERT INTO Banners (Id,Title,Subtitle,BadgeText,Icon,BgGradient,PrimaryLink,PrimaryLinkText,SecondaryLink,SecondaryLinkText,SortOrder,IsActive) VALUES
    (1,'India''s Most Trusted<br><span>Homeopathic Medicines</span>','100% authentic, GMP certified homeopathic remedies prepared with scientific precision for natural healing.','🌿 Trusted Since 1984','fas fa-leaf','linear-gradient(135deg,#0d4a1e 0%,#1a6e2e 60%,#2e8b47 100%)','/Shop','Shop Now','/Home/About','Learn More',1,1),
    (2,'Complete Range of<br><span>Mother Tinctures & Dilutions</span>','From Arnica to Calendula — explore our full catalog of classical homeopathic preparations for every ailment.','🔬 Scientifically Prepared','fas fa-tint','linear-gradient(135deg,#1a3a4e 0%,#1565c0 60%,#1976d2 100%)','/Shop?categoryId=2','Mother Tinctures','/Shop?categoryId=1','View Dilutions',2,1),
    (3,'Free Delivery<br><span>On Orders Above ₹500</span>','Get your medicines delivered to your doorstep across India within 3–5 business days. Pan-India shipping.','🚚 Fast Delivery','fas fa-shipping-fast','linear-gradient(135deg,#3e1a00 0%,#bf360c 60%,#e64a19 100%)','/Shop','Order Now','/Home/Contact','Contact Us',3,1);
    SET IDENTITY_INSERT Banners OFF;
    PRINT 'Banners seeded (3 records).';
END
GO

-- Demo / Admin User
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email='demo@hvip.com')
BEGIN
    INSERT INTO Users (Name,Email,Phone,PasswordHash,Address,City,State,Pincode,IsAdmin,RegisteredOn)
    VALUES ('Demo User','demo@hvip.com','9876543210','2e77f190f9e4e3822824d45086e9ead0fabc7b9b91bdaed29041c24e2ca0028d','15, Green Park','New Delhi','Delhi','110016',1,GETDATE());
    PRINT 'Demo admin user seeded.';
END
ELSE
BEGIN
    UPDATE Users SET IsAdmin=1 WHERE Email='demo@hvip.com';
    PRINT 'Demo user set as admin.';
END
GO

PRINT '=== HVIPDB v3 Setup Complete ===';
GO
