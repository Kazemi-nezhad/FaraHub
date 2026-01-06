using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.DTOs
{
    public class SendMessageDto
    {
        // Content is optional when files are attached. Validate in controller.
        public string Content { get; set; } = string.Empty;
        public List<IFormFile>? Files { get; set; }
    }
}