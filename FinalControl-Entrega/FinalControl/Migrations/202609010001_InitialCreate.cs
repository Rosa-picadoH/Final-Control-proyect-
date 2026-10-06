using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalControl.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Categories", columns: table => new { Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"), Name = table.Column<string>(nullable: false) }, constraints: table => table.PrimaryKey("PK_Categories", x => x.Id));
        migrationBuilder.CreateTable(name: "Users", columns: table => new { Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"), FullName = table.Column<string>(nullable: false), UserName = table.Column<string>(type: "nvarchar(450)", nullable: false), PasswordHash = table.Column<string>(nullable: false), Active = table.Column<bool>(nullable: false) }, constraints: table => table.PrimaryKey("PK_Users", x => x.Id));
        migrationBuilder.CreateTable(name: "Products", columns: table => new { Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"), Code = table.Column<string>(type: "nvarchar(450)", nullable: false), Name = table.Column<string>(nullable: false), Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false), Stock = table.Column<int>(nullable: false), CategoryId = table.Column<int>(nullable: false), Active = table.Column<bool>(nullable: false) }, constraints: table => { table.PrimaryKey("PK_Products", x => x.Id); table.ForeignKey("FK_Products_Categories_CategoryId", x => x.CategoryId, "Categories", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateTable(name: "Sales", columns: table => new { Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"), Date = table.Column<DateTime>(nullable: false), UserId = table.Column<int>(nullable: false), Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false) }, constraints: table => { table.PrimaryKey("PK_Sales", x => x.Id); table.ForeignKey("FK_Sales_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateTable(name: "SaleDetails", columns: table => new { Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"), SaleId = table.Column<int>(nullable: false), ProductId = table.Column<int>(nullable: false), Quantity = table.Column<int>(nullable: false), UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false) }, constraints: table => { table.PrimaryKey("PK_SaleDetails", x => x.Id); table.ForeignKey("FK_SaleDetails_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Restrict); table.ForeignKey("FK_SaleDetails_Sales_SaleId", x => x.SaleId, "Sales", "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateIndex(name: "IX_Products_CategoryId", table: "Products", column: "CategoryId");
        migrationBuilder.CreateIndex(name: "IX_Products_Code", table: "Products", column: "Code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_SaleDetails_ProductId", table: "SaleDetails", column: "ProductId");
        migrationBuilder.CreateIndex(name: "IX_SaleDetails_SaleId", table: "SaleDetails", column: "SaleId");
        migrationBuilder.CreateIndex(name: "IX_Sales_UserId", table: "Sales", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_Users_UserName", table: "Users", column: "UserName", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SaleDetails"); migrationBuilder.DropTable(name: "Products"); migrationBuilder.DropTable(name: "Sales"); migrationBuilder.DropTable(name: "Categories"); migrationBuilder.DropTable(name: "Users");
    }
}
