namespace WholesaleDistributionApp.Models
{
    public class UpdateStockModel
    {
        public string StockId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int? Quantity { get; set; } // Only for Distributor
        public double SinglePrice { get; set; }
        public bool? ForRetailerPurchase { get; set; } // Only for Admin
        public string? DistributorDeliveryStatus { get; set; } // Only for Distributor
        public string StockImage { get; set; }
    }
}
