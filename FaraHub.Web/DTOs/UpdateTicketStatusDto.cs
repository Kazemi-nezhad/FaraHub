using FaraHub.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.DTOs
{
    public class UpdateTicketStatusDto
    {
        [Required]
        public TicketStatus NewStatus { get; set; }
    }
}
