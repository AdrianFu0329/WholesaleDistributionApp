using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class restructureTableOrderDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockDistributorId",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Orders",
                newName: "OrderType");

            migrationBuilder.AddColumn<string>(
                name: "RetailerId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetailerId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "OrderType",
                table: "Orders",
                newName: "WarehouseId");

            migrationBuilder.AddColumn<string>(
                name: "StockDistributorId",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
