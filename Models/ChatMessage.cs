using System.ComponentModel.DataAnnotations;

namespace Website_QLPT.Models
{
    public class ChatMessage : ISoftDelete
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string SenderEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string ReceiverEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
