using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Models
{
    public class AppUser : IdentityUser
    {
        [MaxLength(256)]
        public string? FullName { get; set; }

        public bool IsCompanyMember { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
        public ICollection<Ticket> CustomerTickets { get; set; } = new List<Ticket>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}