using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZ.Infrastructure.Persistence.Migrations.Tenant;

[Migration("20260910132000_RepairCompositionAndValueAddedTables")]
public partial class RepairCompositionAndValueAddedTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ProductCompositions', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.ProductCompositions', N'CompositionName') IS NULL
BEGIN
    DROP TABLE [dbo].[ProductCompositions];
END;
IF OBJECT_ID(N'dbo.ProductCompositions', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductCompositions]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductId] int NOT NULL,
        [CompositionName] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProductCompositions] PRIMARY KEY ([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductCompositions_ProductId_CompositionName' AND object_id = OBJECT_ID(N'dbo.ProductCompositions'))
    CREATE UNIQUE INDEX [IX_ProductCompositions_ProductId_CompositionName] ON [dbo].[ProductCompositions] ([ProductId], [CompositionName]);

IF OBJECT_ID(N'dbo.ProductCompositionLines', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductCompositionLines]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductCompositionId] int NOT NULL,
        [ComponentProductId] int NULL,
        [ComponentName] nvarchar(150) NOT NULL,
        [Quantity] decimal(18,8) NOT NULL,
        [Unit] nvarchar(30) NULL,
        [Percentage] decimal(18,8) NOT NULL,
        [Description] nvarchar(500) NULL,
        [LineNumber] int NOT NULL,
        CONSTRAINT [PK_ProductCompositionLines] PRIMARY KEY ([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductCompositionLines_ProductCompositionId_LineNumber' AND object_id = OBJECT_ID(N'dbo.ProductCompositionLines'))
    CREATE UNIQUE INDEX [IX_ProductCompositionLines_ProductCompositionId_LineNumber] ON [dbo].[ProductCompositionLines] ([ProductCompositionId], [LineNumber]);

IF OBJECT_ID(N'dbo.ValueAddedLists', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ValueAddedLists]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DefaultAmount] decimal(18,8) NOT NULL,
        [AmountType] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ValueAddedLists] PRIMARY KEY ([Id])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ValueAddedLists_Code' AND object_id = OBJECT_ID(N'dbo.ValueAddedLists'))
    CREATE UNIQUE INDEX [IX_ValueAddedLists_Code] ON [dbo].[ValueAddedLists] ([Code]);

IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductCompositions_Products_ProductId')
    ALTER TABLE [dbo].[ProductCompositions] ADD CONSTRAINT [FK_ProductCompositions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]);
IF OBJECT_ID(N'dbo.ProductCompositions', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductCompositionLines_ProductCompositions_ProductCompositionId')
    ALTER TABLE [dbo].[ProductCompositionLines] ADD CONSTRAINT [FK_ProductCompositionLines_ProductCompositions_ProductCompositionId] FOREIGN KEY ([ProductCompositionId]) REFERENCES [dbo].[ProductCompositions] ([Id]) ON DELETE CASCADE;
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductCompositionLines_Products_ComponentProductId')
    ALTER TABLE [dbo].[ProductCompositionLines] ADD CONSTRAINT [FK_ProductCompositionLines_Products_ComponentProductId] FOREIGN KEY ([ComponentProductId]) REFERENCES [dbo].[Products] ([Id]);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ValueAddedLists', N'U') IS NOT NULL DROP TABLE [dbo].[ValueAddedLists];
IF OBJECT_ID(N'dbo.ProductCompositionLines', N'U') IS NOT NULL DROP TABLE [dbo].[ProductCompositionLines'];
IF OBJECT_ID(N'dbo.ProductCompositions', N'U') IS NOT NULL DROP TABLE [dbo].[ProductCompositions'];");
    }
}
