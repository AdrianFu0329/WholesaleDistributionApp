﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class addDistributorStocktoOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StockId",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StockDistributorId",
                table: "DistributorStock",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_StockId",
                table: "OrderDetails",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributorStock_StockDistributorId",
                table: "DistributorStock",
                column: "StockDistributorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributorStock_UserInfo_StockDistributorId",
                table: "DistributorStock",
                column: "StockDistributorId",
                principalTable: "UserInfo",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_DistributorStock_StockId",
                table: "OrderDetails",
                column: "StockId",
                principalTable: "DistributorStock",
                principalColumn: "StockId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DistributorStock_UserInfo_StockDistributorId",
                table: "DistributorStock");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_DistributorStock_StockId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_StockId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_DistributorStock_StockDistributorId",
                table: "DistributorStock");

            migrationBuilder.AlterColumn<string>(
                name: "StockId",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "StockDistributorId",
                table: "DistributorStock",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
