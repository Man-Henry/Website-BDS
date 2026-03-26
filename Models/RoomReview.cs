using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_QLPT.Models
{
    public class RoomReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [Required]
        public int TenantId { get; set; }
        
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        [Display(Name = "Số sao")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Bình luận")]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = true; // Auto approve for now, admin can hide if needed
    }
}
