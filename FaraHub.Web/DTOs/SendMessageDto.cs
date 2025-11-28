using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.DTOs
{
    public class SendMessageDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;
        public List<IFormFile>? Files { get; set; }
    }
}