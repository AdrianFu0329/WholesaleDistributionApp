using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WholesaleDistributionApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributorStockAndUpdateWarehouseStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DistributorDeliveryStatus",
                table: "WarehouseStock",
                newName: "DistributorStockId");

            migrationBuilder.CreateTable(
                name: "DistributorStock",
                columns: table => new
                {
                    StockId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SinglePrice = table.Column<double>(type: "float", nullable: false),
                    StockDistributorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImgDownloadURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistributorDeliveryStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributorStock", x => x.StockId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributorStock");

            migrationBuilder.RenameColumn(
                name: "DistributorStockId",
                table: "WarehouseStock",
                newName: "DistributorDeliveryStatus");
        }
    }
}
