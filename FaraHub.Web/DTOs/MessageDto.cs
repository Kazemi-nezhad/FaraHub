using FaraHub.Web.Controllers;
using static FaraHub.Web.Controllers.TicketController;

namespace FaraHub.Web.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public UserSummaryDto SentBy { get; set; } = null!;
        public List<AttachmentDto>? Attachments { get; set; }
    }
}
