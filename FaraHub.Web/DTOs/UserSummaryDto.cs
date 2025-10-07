namespace FaraHub.Web.Controllers
{
    public class UserSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool IsCompanyMember { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // برای نمایش وضعیت حذف شده
    }
}