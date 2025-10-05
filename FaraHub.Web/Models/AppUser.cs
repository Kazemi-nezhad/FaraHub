using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Models
{
    public class AppUser : IdentityUser
    {
        [MaxLength(256)]
        public string? FullName { get; set; } // مثلاً نام کامل

        public bool IsCompanyMember { get; set; } = false; // مشخص می‌کند آیا کاربر عضو شرکت است یا خیر

        public bool IsActive { get; set; } = true; // فعال/غیرفعال (برای soft delete نیز می‌توان استفاده کرد)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // تاریخ ایجاد
        public DateTime? DeletedAt { get; set; } // تاریخ حذف (soft delete)
    }
}