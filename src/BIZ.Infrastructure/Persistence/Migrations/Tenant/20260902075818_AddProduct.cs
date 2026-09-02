using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZ.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortName",
                table: "Products",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BeforeVat",
                table: "Products",
                type: "decimal(16,6)",
                precision: 16,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DealerPrice",
                table: "Products",
                type: "decimal(16,6)",
                precision: 16,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "Products",
                type: "decimal(16,6)",
                precision: 16,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExciseRate",
                table: "Products",
                type: "decimal(16,6)",
                precision: 16,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInsurableItem",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Margin",
                table: "Products",
                type: "decimal(16,6)",
                precision: 16,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseGLCode",
                table: "Products",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseReturnGLCode",
                table: "Products",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderQty",
                table: "Products",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesGLCode",
                table: "Products",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesReturnGLCode",
                table: "Products",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeforeVat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DealerPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExciseRate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsInsurableItem",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Margin",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseGLCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseReturnGLCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderQty",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalesGLCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalesReturnGLCode",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "ShortName",
                table: "Products",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);
        }
    }
}
