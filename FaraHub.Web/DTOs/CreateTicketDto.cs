using FaraHub.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.DTOs
{
    public class CreateTicketDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        // فقط برای اعضا شرکت - اختیاری
        public string? CustomerId { get; set; }

        // فقط برای اعضا شرکت - اختیاری - باید در API اصلی ایجاد تیکت مدیریت شود، نه اینجا
        //public string? AssignedToId { get; set; } // حذف شد یا شرط‌بندی شود
    }
}
