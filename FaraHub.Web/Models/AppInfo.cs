namespace FaraHub.Web.Models
{
    public class AppInfo
    {
        public int Id { get; set; } // این می‌شود Primary Key
        public string Name { get; set; } = string.Empty; // نام سیستم
        public string Version { get; set; } = string.Empty; // نسخه
    }
}