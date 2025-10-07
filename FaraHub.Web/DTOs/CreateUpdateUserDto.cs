using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Controllers
{
    public class CreateUpdateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public bool IsCompanyMember { get; set; } // مشخص می‌کند آیا عضو شرکت است یا مشتری

        public bool IsActive { get; set; } = true; // فعال یا غیرفعال

        // فقط برای ایجاد
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; } // اختیاری برای ویرایش

        // فقط برای ویرایش
        public List<string>? Roles { get; set; } // لیست نقش‌های جدید
    }
}