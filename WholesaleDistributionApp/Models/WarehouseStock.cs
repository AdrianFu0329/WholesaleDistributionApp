using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WholesaleDistributionApp.Models
{
    public class WarehouseStock
    {
        [Key]
        public string StockId { get; set; }
        public string DistributorStockId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double SinglePrice { get; set; }
        public string StockDistributorId { get; set; }
        public string? ImgDownloadURL {  get; set; }
        public bool ForRetailerPurchase { get; set; }
        public virtual UserInfo Distributor { get; set; }
    }
}
