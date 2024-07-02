using System.ComponentModel.DataAnnotations;

namespace WholesaleDistributionApp.Models
{
    public class RefundRequest
    {
        [Key]
        public int RefundId { get; set; }
        public DateTime RefundDate { get; set; }
        public double RefundAmount { get; set; }
        public string RefundStatus { get; set; }
        public string OrderId { get; set; } // Foreign Key
    }
}
