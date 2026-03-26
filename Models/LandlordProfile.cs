using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_QLPT.Models
{
    public class LandlordProfile
    {
        [Key]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    }

    public enum SubscriptionPlan
    {
        Free, // Max 5 rooms
        Pro,  // Max 20 rooms
        Enterprise // Unlimited
    }
}
