using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrderDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "RefundRequest",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "RefundStatus",
                table: "RefundRequest",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<double>(
                name: "RefundAmount",
                table: "RefundRequest",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "RefundRequest",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "RefundRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefundStatus",
                table: "RefundRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "RefundAmount",
                table: "RefundRequest",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "RefundRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WarehouseStockId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PricePerItem = table.Column<double>(type: "float", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailsId);
                    table.ForeignKey(
                        name: "FK_OrderDetails_DistributorStock_StockId",
                        column: x => x.StockId,
                        principalTable: "DistributorStock",
                        principalColumn: "StockId");
                    table.ForeignKey(
                        name: "FK_OrderDetails_WarehouseStock_WarehouseStockId",
                        column: x => x.WarehouseStockId,
                        principalTable: "WarehouseStock",
                        principalColumn: "StockId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_StockId",
                table: "OrderDetails",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_WarehouseStockId",
                table: "OrderDetails",
                column: "WarehouseStockId");
        }
    }
}
