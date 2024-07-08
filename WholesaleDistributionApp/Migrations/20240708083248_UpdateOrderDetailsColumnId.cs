using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderDetailsColumnId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PricePerItem = table.Column<double>(type: "float", nullable: false),
                    Subtotal = table.Column<double>(type: "float", nullable: false),
                    WarehouseStockId = table.Column<string>(type: "nvarchar(450)", nullable: true)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");
        }
    }
}
