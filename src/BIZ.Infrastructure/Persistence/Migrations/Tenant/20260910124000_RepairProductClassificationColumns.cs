using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIZ.Infrastructure.Persistence.Migrations.Tenant;

[Migration("20260910124000_RepairProductClassificationColumns")]
public partial class RepairProductClassificationColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE [Products] ALTER COLUMN [Category] nvarchar(50) NULL;");
        migrationBuilder.Sql("ALTER TABLE [Products] ALTER COLUMN [ValuationMethod] nvarchar(50) NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE [Products] ALTER COLUMN [Category] nvarchar(1) NULL;");
        migrationBuilder.Sql("ALTER TABLE [Products] ALTER COLUMN [ValuationMethod] nvarchar(1) NULL;");
    }
}
