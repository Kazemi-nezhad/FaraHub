using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Models
{
    public class Login
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // public bool RememberMe { get; set; } = false; // دیگر برای JWT لازم نیست
    }
}
