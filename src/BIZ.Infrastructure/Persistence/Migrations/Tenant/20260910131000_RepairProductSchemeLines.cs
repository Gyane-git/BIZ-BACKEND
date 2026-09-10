using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZ.Infrastructure.Persistence.Migrations.Tenant;

[Migration("20260910131000_RepairProductSchemeLines")]
public partial class RepairProductSchemeLines : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ProductSchemes', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.ProductSchemes', N'SchemeCode') IS NULL
BEGIN
    DROP TABLE [dbo].[ProductSchemes];
END;

IF OBJECT_ID(N'dbo.ProductSchemes', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductSchemes]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [SchemeCode] nvarchar(50) NOT NULL,
        [SchemeName] nvarchar(150) NOT NULL,
        [SchemeType] nvarchar(30) NOT NULL,
        [FromDate] datetime2 NOT NULL,
        [ToDate] datetime2 NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProductSchemes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductSchemes_SchemeCode' AND object_id = OBJECT_ID(N'dbo.ProductSchemes'))
    CREATE UNIQUE INDEX [IX_ProductSchemes_SchemeCode] ON [dbo].[ProductSchemes] ([SchemeCode]);

IF OBJECT_ID(N'dbo.ProductSchemeLines', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductSchemeLines]
    (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ProductSchemeId] int NOT NULL,
        [ProductId] int NOT NULL,
        [MinimumQuantity] decimal(18,8) NOT NULL,
        [MaximumQuantity] decimal(18,8) NULL,
        [DiscountPercent] decimal(18,8) NOT NULL,
        [DiscountAmount] decimal(18,8) NOT NULL,
        [FreeQuantity] decimal(18,8) NOT NULL,
        [Description] nvarchar(500) NULL,
        [LineNumber] int NOT NULL,
        CONSTRAINT [PK_ProductSchemeLines] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductSchemeLines_ProductSchemeId_LineNumber' AND object_id = OBJECT_ID(N'dbo.ProductSchemeLines'))
    CREATE UNIQUE INDEX [IX_ProductSchemeLines_ProductSchemeId_LineNumber] ON [dbo].[ProductSchemeLines] ([ProductSchemeId], [LineNumber]);
IF OBJECT_ID(N'dbo.ProductSchemes', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductSchemeLines_ProductSchemes_ProductSchemeId')
    ALTER TABLE [dbo].[ProductSchemeLines] ADD CONSTRAINT [FK_ProductSchemeLines_ProductSchemes_ProductSchemeId] FOREIGN KEY ([ProductSchemeId]) REFERENCES [dbo].[ProductSchemes] ([Id]) ON DELETE CASCADE;
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductSchemeLines_Products_ProductId')
    ALTER TABLE [dbo].[ProductSchemeLines] ADD CONSTRAINT [FK_ProductSchemeLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.ProductSchemeLines', N'U') IS NOT NULL DROP TABLE [dbo].[ProductSchemeLines];
IF OBJECT_ID(N'dbo.ProductSchemes', N'U') IS NOT NULL DROP TABLE [dbo].[ProductSchemes];");
    }
}
