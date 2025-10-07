namespace FaraHub.Web.Controllers
{
    public class UserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool IsCompanyMember { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        // می‌توانید فیلدهای دیگری مانند تعداد تیکت‌ها یا زمان کار اضافه کنید
    }
}