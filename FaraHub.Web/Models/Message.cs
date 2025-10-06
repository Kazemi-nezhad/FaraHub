using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace FaraHub.Web.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // ارتباطات
        [Required] // پیام باید به یک تیکت تعلق داشته باشد
        public int TicketId { get; set; } // FK به Ticket
        public Ticket Ticket { get; set; } = null!; // Navigation Property

        [Required] // پیام باید توسط کاربری ارسال شود
        public string SentById { get; set; } = string.Empty; // FK به AppUser
        public AppUser SentBy { get; set; } = null!; // Navigation Property

        // لیست فایل‌های پیوست شده به این پیام
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        // Soft Delete (اختیاری - ممکن است بخواهید پیام‌ها را هم نگه دارید)
        public DateTime? DeletedAt { get; set; } = null;
    }
}