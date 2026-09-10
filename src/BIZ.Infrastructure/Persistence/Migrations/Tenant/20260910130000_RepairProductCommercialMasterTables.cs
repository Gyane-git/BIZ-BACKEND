using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZ.Infrastructure.Persistence.Migrations.Tenant;

[Migration("20260910130000_RepairProductCommercialMasterTables")]
public partial class RepairProductCommercialMasterTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ProductSchemes', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductSchemes]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [SchemeType] nvarchar(30) NOT NULL,
        [ProductId] int NULL,
        [ProductGroupCode] nvarchar(50) NULL,
        [ProductSubGroupCode] nvarchar(50) NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [DiscountPercentage] decimal(18,8) NULL,
        [DiscountAmount] decimal(18,8) NULL,
        [MinimumQuantity] decimal(18,8) NULL,
        [FreeQuantity] decimal(18,8) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProductSchemes] PRIMARY KEY ([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductSchemes_Code' AND object_id = OBJECT_ID(N'dbo.ProductSchemes'))
    CREATE UNIQUE INDEX [IX_ProductSchemes_Code] ON [dbo].[ProductSchemes] ([Code]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductSchemes_ProductId' AND object_id = OBJECT_ID(N'dbo.ProductSchemes'))
    CREATE INDEX [IX_ProductSchemes_ProductId] ON [dbo].[ProductSchemes] ([ProductId]);

IF OBJECT_ID(N'dbo.ProductCompositions', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductCompositions]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ParentProductId] int NOT NULL,
        [ComponentProductId] int NOT NULL,
        [Quantity] decimal(18,8) NOT NULL,
        [WastagePercentage] decimal(18,8) NULL,
        [UnitId] int NULL,
        [LineNumber] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProductCompositions] PRIMARY KEY ([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductCompositions_ParentProductId_ComponentProductId' AND object_id = OBJECT_ID(N'dbo.ProductCompositions'))
    CREATE UNIQUE INDEX [IX_ProductCompositions_ParentProductId_ComponentProductId] ON [dbo].[ProductCompositions] ([ParentProductId], [ComponentProductId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductCompositions_ComponentProductId' AND object_id = OBJECT_ID(N'dbo.ProductCompositions'))
    CREATE INDEX [IX_ProductCompositions_ComponentProductId] ON [dbo].[ProductCompositions] ([ComponentProductId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductCompositions_UnitId' AND object_id = OBJECT_ID(N'dbo.ProductCompositions'))
    CREATE INDEX [IX_ProductCompositions_UnitId] ON [dbo].[ProductCompositions] ([UnitId]);

IF OBJECT_ID(N'dbo.ProductValueAdded', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductValueAdded]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductId] int NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Quantity] decimal(18,8) NOT NULL,
        [UnitCost] decimal(18,8) NOT NULL,
        [IsPercentage] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProductValueAdded] PRIMARY KEY ([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductValueAdded_ProductId_Name' AND object_id = OBJECT_ID(N'dbo.ProductValueAdded'))
    CREATE UNIQUE INDEX [IX_ProductValueAdded_ProductId_Name] ON [dbo].[ProductValueAdded] ([ProductId], [Name]);

IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductSchemes_Products_ProductId')
    ALTER TABLE [dbo].[ProductSchemes] ADD CONSTRAINT [FK_ProductSchemes_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]);
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductCompositions_Products_ParentProductId')
    ALTER TABLE [dbo].[ProductCompositions] ADD CONSTRAINT [FK_ProductCompositions_Products_ParentProductId] FOREIGN KEY ([ParentProductId]) REFERENCES [dbo].[Products] ([Id]);
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductCompositions_Products_ComponentProductId')
    ALTER TABLE [dbo].[ProductCompositions] ADD CONSTRAINT [FK_ProductCompositions_Products_ComponentProductId] FOREIGN KEY ([ComponentProductId]) REFERENCES [dbo].[Products] ([Id]);
IF OBJECT_ID(N'dbo.Units', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductCompositions_Units_UnitId')
    ALTER TABLE [dbo].[ProductCompositions] ADD CONSTRAINT [FK_ProductCompositions_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[Units] ([Id]);
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductValueAdded_Products_ProductId')
    ALTER TABLE [dbo].[ProductValueAdded] ADD CONSTRAINT [FK_ProductValueAdded_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ProductValueAdded', N'U') IS NOT NULL DROP TABLE [dbo].[ProductValueAdded];
IF OBJECT_ID(N'dbo.ProductCompositions', N'U') IS NOT NULL DROP TABLE [dbo].[ProductCompositions];
IF OBJECT_ID(N'dbo.ProductSchemes', N'U') IS NOT NULL DROP TABLE [dbo].[ProductSchemes];");
    }
}
