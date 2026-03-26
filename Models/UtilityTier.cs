using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_QLPT.Models
{
    public enum UtilityType
    {
        [Display(Name = "Điện")]
        Electricity = 0,
        [Display(Name = "Nước")]
        Water = 1
    }

    public class UtilityTier
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public UtilityType Type { get; set; } = UtilityType.Electricity;

        [Display(Name = "Chỉ số từ")]
        public int FromUnit { get; set; }

        [Display(Name = "Chỉ số đến (để trống nếu vô hạn)")]
        public int? ToUnit { get; set; }

        [Required]
        [Display(Name = "Đơn giá (VND)")]
        [Column(TypeName = "decimal(10,0)")]
        public decimal Price { get; set; }

        // Mối quan hệ với Property
        public virtual Property? Property { get; set; }
    }
}
