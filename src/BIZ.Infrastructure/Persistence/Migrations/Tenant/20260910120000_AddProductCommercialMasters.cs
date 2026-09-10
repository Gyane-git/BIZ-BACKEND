using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZ.Infrastructure.Persistence.Migrations.Tenant;

[Migration("20260910120000_AddProductCommercialMasters")]
public partial class AddProductCommercialMasters : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProductSchemes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SchemeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ProductId = table.Column<int>(type: "int", nullable: true),
                ProductGroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ProductSubGroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                DiscountPercentage = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                DiscountAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                MinimumQuantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                FreeQuantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ProductSchemes", x => x.Id);
                table.ForeignKey("FK_ProductSchemes_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductCompositions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ParentProductId = table.Column<int>(type: "int", nullable: false),
                ComponentProductId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                WastagePercentage = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                UnitId = table.Column<int>(type: "int", nullable: true),
                LineNumber = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ProductCompositions", x => x.Id);
                table.ForeignKey("FK_ProductCompositions_Products_ParentProductId", x => x.ParentProductId, "Products", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ProductCompositions_Products_ComponentProductId", x => x.ComponentProductId, "Products", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ProductCompositions_Units_UnitId", x => x.UnitId, "Units", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductValueAdded",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Quantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                UnitCost = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ProductValueAdded", x => x.Id);
                table.ForeignKey("FK_ProductValueAdded_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_ProductSchemes_Code", "ProductSchemes", "Code", unique: true);
        migrationBuilder.CreateIndex("IX_ProductSchemes_ProductId", "ProductSchemes", "ProductId");
        migrationBuilder.CreateIndex("IX_ProductCompositions_ParentProductId_ComponentProductId", "ProductCompositions", new[] { "ParentProductId", "ComponentProductId" }, unique: true);
        migrationBuilder.CreateIndex("IX_ProductCompositions_ComponentProductId", "ProductCompositions", "ComponentProductId");
        migrationBuilder.CreateIndex("IX_ProductCompositions_UnitId", "ProductCompositions", "UnitId");
        migrationBuilder.CreateIndex("IX_ProductValueAdded_ProductId_Name", "ProductValueAdded", new[] { "ProductId", "Name" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ProductValueAdded");
        migrationBuilder.DropTable(name: "ProductCompositions");
        migrationBuilder.DropTable(name: "ProductSchemes");
    }
}
