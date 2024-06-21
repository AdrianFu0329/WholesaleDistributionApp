using System.ComponentModel.DataAnnotations;

namespace WholesaleDistributionApp.Models
{
    public class UserInfo
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string UserRole { get; set; }
    }
}
