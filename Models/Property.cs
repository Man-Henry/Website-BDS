using System.ComponentModel.DataAnnotations;

namespace Website_QLPT.Models
{
    public class Property : ISoftDelete
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nhà/dãy trọ không được để trống.")]
        [Display(Name = "Tên Nhà/Dãy Trọ")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        [MaxLength(300)]
        public string? Address { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Chủ nhà")]
        public string? OwnerId { get; set; }
        public virtual Microsoft.AspNetCore.Identity.IdentityUser? Owner { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Navigation properties
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        [Display(Name = "Tính tiền điện bậc thang")]
        public bool IsElectricityTiered { get; set; } = false;

        public virtual ICollection<UtilityTier> UtilityTiers { get; set; } = new List<UtilityTier>();

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
