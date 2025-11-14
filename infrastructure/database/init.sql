-- Wait for SQL Server to be ready
PRINT 'Starting database initialization...';
GO

-- Create the database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductsDB')
BEGIN
    CREATE DATABASE ProductsDB;
    PRINT 'Database ProductsDB created';
END
ELSE
BEGIN
    PRINT 'Database ProductsDB already exists';
END
GO

USE ProductsDB;
GO

-- Create the Product table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(MAX),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(255) NULL,
        CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastModifiedBy NVARCHAR(255) NULL,
        LastModifiedOn DATETIME2 NULL,
        RowVersion ROWVERSION NOT NULL
    );
    PRINT 'Products table created';
END
ELSE
BEGIN
    PRINT 'Products table already exists';
    -- Add missing columns if table exists but columns are missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'CreatedBy')
    BEGIN
        ALTER TABLE Products ADD CreatedBy NVARCHAR(255) NULL;
        PRINT 'Added CreatedBy column';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'CreatedOn')
    BEGIN
        ALTER TABLE Products ADD CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added CreatedOn column';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'LastModifiedBy')
    BEGIN
        ALTER TABLE Products ADD LastModifiedBy NVARCHAR(255) NULL;
        PRINT 'Added LastModifiedBy column';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'LastModifiedOn')
    BEGIN
        ALTER TABLE Products ADD LastModifiedOn DATETIME2 NULL;
        PRINT 'Added LastModifiedOn column';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'RowVersion')
    BEGIN
        ALTER TABLE Products ADD RowVersion ROWVERSION NOT NULL;
        PRINT 'Added RowVersion column';
    END
END
GO

-- Insert seed data only if table is empty
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (Name, Price, Description, UpdatedAt, CreatedOn)
    VALUES 
        ('Laptop Pro 15"', 1299.99, 'High-performance laptop with 16GB RAM and 512GB SSD', GETUTCDATE(), GETUTCDATE()),
        ('Wireless Mouse', 29.99, 'Ergonomic wireless mouse with USB receiver', GETUTCDATE(), GETUTCDATE()),
        ('Mechanical Keyboard', 89.99, 'RGB mechanical keyboard with blue switches', GETUTCDATE(), GETUTCDATE()),
        ('USB-C Hub', 49.99, '7-in-1 USB-C hub with HDMI and ethernet', GETUTCDATE(), GETUTCDATE()),
        ('Webcam HD', 79.99, '1080p webcam with built-in microphone', GETUTCDATE(), GETUTCDATE()),
        ('Monitor 27"', 299.99, '27-inch 4K monitor with IPS panel', GETUTCDATE(), GETUTCDATE()),
        ('Desk Lamp', 34.99, 'LED desk lamp with adjustable brightness', GETUTCDATE(), GETUTCDATE()),
        ('External SSD 1TB', 119.99, 'Portable SSD with USB 3.1 Gen 2', GETUTCDATE(), GETUTCDATE());
    
    PRINT '8 products inserted';
END
ELSE
BEGIN
    PRINT 'Seed data already exists';
END
GO

-- Enable CDC on the database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductsDB' AND is_cdc_enabled = 1)
BEGIN
    EXEC sys.sp_cdc_enable_db;
    PRINT 'CDC enabled on ProductsDB';
END
ELSE
BEGIN
    PRINT 'CDC already enabled on ProductsDB';
END
GO

-- Enable CDC on the Products table
IF NOT EXISTS (
    SELECT * FROM sys.tables t
    JOIN cdc.change_tables ct ON t.object_id = ct.source_object_id
    WHERE t.name = 'Products'
)
BEGIN
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'dbo',
        @source_name = N'Products',
        @role_name = NULL,
        @supports_net_changes = 1;
    PRINT 'CDC enabled on Products table';
END
ELSE
BEGIN
    PRINT 'CDC already enabled on Products table';
END
GO

PRINT 'Database initialization completed successfully!';
GO

