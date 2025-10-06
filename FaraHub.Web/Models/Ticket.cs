using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.InProgress;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivityAt { get; set; }

        // ارتباطات
        [Required] // الزامی - تیکت باید توسط کاربری ایجاد شود
        public string CreatedById { get; set; } = string.Empty;
        public AppUser CreatedBy { get; set; } = null!; // Navigation Property

        public string? AssignedToId { get; set; } // اختیاری - ممکن است ارجاع نشده باشد
        public AppUser? AssignedTo { get; set; } // Navigation Property (اختیاری)

        public string? CustomerId { get; set; } // اختیاری - ممکن است مشتری خاصی نداشته باشد
        public AppUser? Customer { get; set; } // Navigation Property (اختیاری)

        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();

        // Soft Delete
        public DateTime? DeletedAt { get; set; } = null;
    }
}