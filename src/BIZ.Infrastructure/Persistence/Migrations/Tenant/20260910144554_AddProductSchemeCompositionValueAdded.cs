using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZ.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddProductSchemeCompositionValueAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValuationMethod",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCompositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CompositionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCompositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCompositions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductSchemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchemeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SchemeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSchemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValueAddedLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    AmountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueAddedLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCompositionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCompositionId = table.Column<int>(type: "int", nullable: false),
                    ComponentProductId = table.Column<int>(type: "int", nullable: true),
                    ComponentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LineNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCompositionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCompositionLines_ProductCompositions_ProductCompositionId",
                        column: x => x.ProductCompositionId,
                        principalTable: "ProductCompositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCompositionLines_Products_ComponentProductId",
                        column: x => x.ComponentProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductSchemeLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductSchemeId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MinimumQuantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MaximumQuantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    FreeQuantity = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LineNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSchemeLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSchemeLines_ProductSchemes_ProductSchemeId",
                        column: x => x.ProductSchemeId,
                        principalTable: "ProductSchemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSchemeLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositionLines_ComponentProductId",
                table: "ProductCompositionLines",
                column: "ComponentProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositionLines_ProductCompositionId_LineNumber",
                table: "ProductCompositionLines",
                columns: new[] { "ProductCompositionId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositions_ProductId_CompositionName",
                table: "ProductCompositions",
                columns: new[] { "ProductId", "CompositionName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSchemeLines_ProductId",
                table: "ProductSchemeLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSchemeLines_ProductSchemeId_LineNumber",
                table: "ProductSchemeLines",
                columns: new[] { "ProductSchemeId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSchemes_SchemeCode",
                table: "ProductSchemes",
                column: "SchemeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValueAddedLists_Code",
                table: "ValueAddedLists",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCompositionLines");

            migrationBuilder.DropTable(
                name: "ProductSchemeLines");

            migrationBuilder.DropTable(
                name: "ValueAddedLists");

            migrationBuilder.DropTable(
                name: "ProductCompositions");

            migrationBuilder.DropTable(
                name: "ProductSchemes");

            migrationBuilder.AlterColumn<string>(
                name: "ValuationMethod",
                table: "Products",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
