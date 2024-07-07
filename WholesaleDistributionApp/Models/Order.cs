using System;
using System.ComponentModel.DataAnnotations;

namespace WholesaleDistributionApp.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string StockDistributorId { get; set; }
        public double TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentReceiptURL { get; set; }
        public string OrderType { get; set; } // "Warehouse" or "Retailer"
        public string? RetailerId { get; set; }
    }
}
