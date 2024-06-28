using System.ComponentModel.DataAnnotations;

namespace WholesaleDistributionApp.Models
{
    public class OrderDetails
    {
        [Key]
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public string StockId { get; set; }
        public int Quantity { get; set; }
        public double PricePerItem { get; set; }
        public double Subtotal { get; set; }
    }
}
