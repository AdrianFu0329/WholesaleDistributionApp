using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOrderIdToGuidAndMoveItForward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders_New",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    WarehouseId = table.Column<string>(nullable: true),
                    StockDistributorId = table.Column<string>(nullable: true),
                    TotalAmount = table.Column<double>(nullable: false),
                    OrderStatus = table.Column<string>(nullable: true),
                    PaymentReceiptURL = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders_New", x => x.OrderId);
                });

            migrationBuilder.Sql("INSERT INTO Orders_New (OrderId, OrderDate, WarehouseId, StockDistributorId, TotalAmount, OrderStatus, PaymentReceiptURL) " +
                                 "SELECT OrderId, OrderDate, WarehouseId, StockDistributorId, TotalAmount, OrderStatus, PaymentReceiptURL FROM Orders");

            migrationBuilder.DropTable("Orders");

            migrationBuilder.RenameTable("Orders_New", newName: "Orders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders_New",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    WarehouseId = table.Column<string>(nullable: true),
                    StockDistributorId = table.Column<string>(nullable: true),
                    TotalAmount = table.Column<double>(nullable: false),
                    OrderStatus = table.Column<string>(nullable: true),
                    PaymentReceiptURL = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders_New", x => x.OrderId);
                });

            migrationBuilder.Sql("INSERT INTO Orders_New (OrderId, OrderDate, WarehouseId, StockDistributorId, TotalAmount, OrderStatus, PaymentReceiptURL) " +
                                 "SELECT OrderId, OrderDate, WarehouseId, StockDistributorId, TotalAmount, OrderStatus, PaymentReceiptURL FROM Orders");

            migrationBuilder.DropTable("Orders");

            migrationBuilder.RenameTable("Orders_New", newName: "Orders");
        }
    }
}
