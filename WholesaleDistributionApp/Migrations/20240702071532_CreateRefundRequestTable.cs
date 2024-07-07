using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class CreateRefundRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StockDistributorId",
                table: "WarehouseStock",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StockId",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "RefundRequest",
                columns: table => new
                {
                    RefundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefundAmount = table.Column<double>(type: "float", nullable: false),
                    RefundStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequest", x => x.RefundId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseStock_StockDistributorId",
                table: "WarehouseStock",
                column: "StockDistributorId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseStock_UserInfo_StockDistributorId",
                table: "WarehouseStock",
                column: "StockDistributorId",
                principalTable: "UserInfo",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseStock_UserInfo_StockDistributorId",
                table: "WarehouseStock");

            migrationBuilder.DropTable(
                name: "RefundRequest");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseStock_StockDistributorId",
                table: "WarehouseStock");

            migrationBuilder.AlterColumn<string>(
                name: "StockDistributorId",
                table: "WarehouseStock",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "StockId",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
