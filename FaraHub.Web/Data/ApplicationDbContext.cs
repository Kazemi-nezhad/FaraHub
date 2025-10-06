using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FaraHub.Web.Models;

namespace FaraHub.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppInfo> AppInfos { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Soft Delete برای AppUser
            builder.Entity<AppUser>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Soft Delete برای مدل‌های تیکت
            builder.Entity<Ticket>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                // ارتباط چند به یک با AppUser (CreatedById) - الزامی
                entity.HasOne(t => t.CreatedBy)
                      .WithMany(u => u.CreatedTickets)
                      .HasForeignKey(t => t.CreatedById)
                      .OnDelete(DeleteBehavior.NoAction); // NoAction برای همه

                // ارتباط چند به یک با AppUser (AssignedToId) - اختیاری
                entity.HasOne(t => t.AssignedTo)
                      .WithMany(u => u.AssignedTickets)
                      .HasForeignKey(t => t.AssignedToId)
                      .OnDelete(DeleteBehavior.NoAction); // تغییر از SetNull به NoAction

                // ارتباط چند به یک با AppUser (CustomerId) - اختیاری
                entity.HasOne(t => t.Customer)
                      .WithMany(u => u.CustomerTickets)
                      .HasForeignKey(t => t.CustomerId)
                      .OnDelete(DeleteBehavior.NoAction); // تغییر از SetNull به NoAction
            });

            builder.Entity<Message>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                // ارتباط چند به یک با Ticket
                entity.HasOne(m => m.Ticket)
                      .WithMany(t => t.Messages)
                      .HasForeignKey(m => m.TicketId)
                      .OnDelete(DeleteBehavior.Cascade); // این Ok است، چون Ticket فیلتر دارد

                // ارتباط چند به یک با AppUser
                entity.HasOne(m => m.SentBy)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SentById)
                      .OnDelete(DeleteBehavior.NoAction); // NoAction
            });

            builder.Entity<Attachment>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                // ارتباط چند به یک با Message
                entity.HasOne(a => a.Message)
                      .WithMany(m => m.Attachments)
                      .HasForeignKey(a => a.MessageId)
                      .OnDelete(DeleteBehavior.Cascade); // Ok
            });

            builder.Entity<TimeLog>(entity =>
            {
                // Soft Delete برای TimeLog اضافه شد
                entity.HasQueryFilter(e => e.DeletedAt == null);

                // ارتباط چند به یک با Ticket - الزامی
                entity.HasOne(tl => tl.Ticket)
                      .WithMany(t => t.TimeLogs)
                      .HasForeignKey(tl => tl.TicketId)
                      .OnDelete(DeleteBehavior.NoAction); // تغییر از Cascade به NoAction برای جلوگیری از تناقض با فیلتر Ticket

                // ارتباط چند به یک با AppUser - الزامی
                entity.HasOne(tl => tl.User)
                      .WithMany(u => u.TimeLogs)
                      .HasForeignKey(tl => tl.UserId)
                      .OnDelete(DeleteBehavior.NoAction); // NoAction
            });
        }
    }
}