using FaraHub.Web.Controllers;
using FaraHub.Web.Models;

namespace FaraHub.Web.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public UserSummaryDto CreatedBy { get; set; } = null!;
        public UserSummaryDto? AssignedTo { get; set; }
        public UserSummaryDto? Customer { get; set; }
    }
}
