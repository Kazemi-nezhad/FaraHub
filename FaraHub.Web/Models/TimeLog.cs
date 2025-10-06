using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Models
{
    public class TimeLog
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; } // FK به Ticket
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty; // FK به AppUser
        public AppUser User { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } // ممکن است خالی باشد اگر زمان هنوز متوقف نشده
        public TimeSpan? TotalDuration { get; set; } // می‌تواند محاسبه شود یا ذخیره شود

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Soft Delete (احتمالاً لازم نیست، اما اگر بخواهید تاریخچه را نگه دارید و قابل حذف باشد)
        // public DateTime? DeletedAt { get; set; } = null;
    }
}