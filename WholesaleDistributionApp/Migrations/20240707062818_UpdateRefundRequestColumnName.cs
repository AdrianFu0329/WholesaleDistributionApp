using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRefundRequestColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_DistributorStock_StockId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_WarehouseStock_WarehouseStockId",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "RefundDate",
                table: "RefundRequest",
                newName: "RequestDate");

            migrationBuilder.AlterColumn<string>(
                name: "WarehouseStockId",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "StockId",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_DistributorStock_StockId",
                table: "OrderDetails",
                column: "StockId",
                principalTable: "DistributorStock",
                principalColumn: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_WarehouseStock_WarehouseStockId",
                table: "OrderDetails",
                column: "WarehouseStockId",
                principalTable: "WarehouseStock",
                principalColumn: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_DistributorStock_StockId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_WarehouseStock_WarehouseStockId",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "RefundRequest",
                newName: "RefundDate");

            migrationBuilder.AlterColumn<string>(
                name: "WarehouseStockId",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StockId",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_DistributorStock_StockId",
                table: "OrderDetails",
                column: "StockId",
                principalTable: "DistributorStock",
                principalColumn: "StockId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_WarehouseStock_WarehouseStockId",
                table: "OrderDetails",
                column: "WarehouseStockId",
                principalTable: "WarehouseStock",
                principalColumn: "StockId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
