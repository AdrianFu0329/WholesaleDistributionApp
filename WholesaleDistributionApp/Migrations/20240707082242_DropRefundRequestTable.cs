using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class DropRefundRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefundRequest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefundRequest",
                columns: table => new
                {
                    RefundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefundAmount = table.Column<double>(type: "float", nullable: false),
                    RefundStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequest", x => x.RefundId);
                });
        }
    }
}
