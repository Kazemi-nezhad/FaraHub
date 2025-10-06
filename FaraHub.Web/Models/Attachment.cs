using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)] // نام فایل
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)] // مسیر ذخیره فایل (نسبی یا مطلق)
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public long Size { get; set; } // اندازه فایل به بایت

        [Required]
        [MaxLength(100)] // نوع فایل (مثلاً image/png, application/pdf)
        public string ContentType { get; set; } = string.Empty;

        // ارتباطات
        [Required] // فایل باید به یک پیام تعلق داشته باشد
        public int MessageId { get; set; } // FK به Message
        public Message Message { get; set; } = null!; // Navigation Property

        // Soft Delete
        public DateTime? DeletedAt { get; set; } = null;
    }
}