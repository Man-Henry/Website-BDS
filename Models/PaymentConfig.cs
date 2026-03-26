using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Website_QLPT.Models
{
    public class PaymentConfig
    {
        public int Id { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cổng thanh toán")]
        public PaymentProvider Provider { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = false;

        [Required]
        [Display(Name = "Cấu hình (Mã hóa)")]
        public string ConfigData { get; set; } = string.Empty;

        // Navigation property
        public virtual IdentityUser? Owner { get; set; }
    }
}