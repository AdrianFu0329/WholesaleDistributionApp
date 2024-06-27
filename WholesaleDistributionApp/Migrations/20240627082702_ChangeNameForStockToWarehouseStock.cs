using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameForStockToWarehouseStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.CreateTable(
                name: "WarehouseStock",
                columns: table => new
                {
                    StockId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SinglePrice = table.Column<double>(type: "float", nullable: false),
                    StockDistributorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImgDownloadURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForRetailerPurchase = table.Column<bool>(type: "bit", nullable: false),
                    DistributorDeliveryStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseStock", x => x.StockId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarehouseStock");

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    StockId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistributorDeliveryStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForRetailerPurchase = table.Column<bool>(type: "bit", nullable: false),
                    ImgDownloadURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SinglePrice = table.Column<double>(type: "float", nullable: false),
                    StockDistributorId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.StockId);
                });
        }
    }
}
