using System.ComponentModel.DataAnnotations;

namespace Website_QLPT.Models
{
    public class MaintenanceRequest : ISoftDelete
    {
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        [Required]
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public enum MaintenanceStatus
    {
        Pending = 0,
        InProgress = 1,
        Resolved = 2
    }
}
