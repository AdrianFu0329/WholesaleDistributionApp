using System;
using System.ComponentModel.DataAnnotations;

namespace WholesaleDistributionApp.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string WarehouseId { get; set; }
        public string StockDistributorId { get; set; }
        public double TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentReceiptURL { get; set; }
    }
}
