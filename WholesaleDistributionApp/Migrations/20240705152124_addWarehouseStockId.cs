using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class addWarehouseStockId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WarehouseStockId",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_WarehouseStockId",
                table: "OrderDetails",
                column: "WarehouseStockId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_WarehouseStock_WarehouseStockId",
                table: "OrderDetails",
                column: "WarehouseStockId",
                principalTable: "WarehouseStock",
                principalColumn: "StockId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_WarehouseStock_WarehouseStockId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_WarehouseStockId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "WarehouseStockId",
                table: "OrderDetails");
        }
    }
}
