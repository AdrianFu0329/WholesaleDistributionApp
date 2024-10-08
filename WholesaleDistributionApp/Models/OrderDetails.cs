﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleDistributionApp.Models
{
    public class OrderDetails
    {
        [Key]
        public Guid OrderDetailsId { get; set; }
        public Guid OrderId { get; set; }
        public string? StockId { get; set; }
        public int Quantity { get; set; }
        public double PricePerItem { get; set; }
        public double Subtotal { get; set; }
        public string? WarehouseStockId { get; set; }

        [ForeignKey("StockId")]
        public virtual DistributorStock DistributorStock { get; set; }

        [ForeignKey("WarehouseStockId")]
        public virtual WarehouseStock WarehouseStock { get; set; }
    }
}
