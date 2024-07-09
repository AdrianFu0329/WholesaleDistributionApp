using System.ComponentModel.DataAnnotations;

namespace WholesaleDistributionApp.Models
{
    public class RefundRequest
    {
        [Key]
        public string? RefundId { get; set; }
        public DateTime? RequestDate { get; set; }
        public double? RefundAmount { get; set; }
        public string? RefundStatus { get; set; }
        public string? RefundType { get; set; }
        public string? OrderId { get; set; } // Foreign Key
    }
}
