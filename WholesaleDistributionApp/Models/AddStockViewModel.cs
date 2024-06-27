namespace WholesaleDistributionApp.Models
{
    public class AddStockViewModel
    {
        public string? DistributorStockId { get; set; } //When add to Warehouse Stock List
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double SinglePrice { get; set; }
        public string? StockDistributorId { get; set; }
        public string? ImgDownloadURL { get; set; }
        public bool? ForRetailerPurchase { get; set; } // Only for Admin 
        public string? DistributorDeliveryStatus { get; set; } // Only for Distributor
    }
}
