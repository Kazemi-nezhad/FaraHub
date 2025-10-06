using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Models
{
    public class TimeLog
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? TotalDuration { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete اضافه شد
        public DateTime? DeletedAt { get; set; } = null;
    }
}