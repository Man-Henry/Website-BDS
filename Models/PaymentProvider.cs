using System.ComponentModel.DataAnnotations;

namespace Website_QLPT.Models
{
    public enum PaymentProvider
    {
        [Display(Name = "Ví MoMo")]
        MoMo = 1,
        
        [Display(Name = "VietQR (PayOS)")]
        PayOS = 2,
        
        [Display(Name = "VNPay")]
        VNPay = 3
    }
}