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
                      .WithMany(u => u.CreatedTickets) // نیاز به تعریف این Navigation Property در AppUser داریم
                      .HasForeignKey(t => t.CreatedById)
                      .OnDelete(DeleteBehavior.NoAction); // اصلی‌ترین تغییر: اطمینان از NoAction برای کلید اصلی

                // ارتباط چند به یک با AppUser (AssignedToId) - اختیاری
                entity.HasOne(t => t.AssignedTo)
                      .WithMany(u => u.AssignedTickets) // نیاز به تعریف این Navigation Property در AppUser داریم
                      .HasForeignKey(t => t.AssignedToId)
                      .OnDelete(DeleteBehavior.SetNull); // درست است

                // ارتباط چند به یک با AppUser (CustomerId) - اختیاری
                entity.HasOne(t => t.Customer)
                      .WithMany(u => u.CustomerTickets) // نیاز به تعریف این Navigation Property در AppUser داریم
                      .HasForeignKey(t => t.CustomerId)
                      .OnDelete(DeleteBehavior.SetNull); // اصلی‌ترین تغییر: اطمینان از SetNull و اینکه CustomerId اختیاری است
            });

            builder.Entity<Message>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                // ارتباط چند به یک با Ticket
                entity.HasOne(m => m.Ticket)
                      .WithMany(t => t.Messages)
                      .HasForeignKey(m => m.TicketId)
                      .OnDelete(DeleteBehavior.Cascade); // اگر تیکت حذف شود، پیام‌ها هم حذف شوند (Soft Delete)

                // ارتباط چند به یک با AppUser
                entity.HasOne(m => m.SentBy)
                      .WithMany(u => u.SentMessages) // نیاز به تعریف این Navigation Property در AppUser داریم
                      .HasForeignKey(m => m.SentById)
                      .OnDelete(DeleteBehavior.NoAction); // یا Restrict - منطق مشابه CreatedBy
            });

            builder.Entity<Attachment>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null);
                // ارتباط چند به یک با Message
                entity.HasOne(a => a.Message)
                      .WithMany(m => m.Attachments)
                      .HasForeignKey(a => a.MessageId)
                      .OnDelete(DeleteBehavior.Cascade); // اگر پیام حذف شود، فایل پیوست هم حذف شود (Soft Delete)
            });

            builder.Entity<TimeLog>(entity =>
            {
                // Soft Delete برای TimeLog نیز اگر لازم باشد
                // entity.HasQueryFilter(e => e.DeletedAt == null);

                // ارتباط چند به یک با Ticket
                entity.HasOne(tl => tl.Ticket)
                      .WithMany(t => t.TimeLogs)
                      .HasForeignKey(tl => tl.TicketId)
                      .OnDelete(DeleteBehavior.Cascade); // اگر تیکت حذف شود، زمان‌ها هم حذف شوند

                // ارتباط چند به یک با AppUser
                entity.HasOne(tl => tl.User)
                      .WithMany(u => u.TimeLogs) // نیاز به تعریف این Navigation Property در AppUser داریم
                      .HasForeignKey(tl => tl.UserId)
                      .OnDelete(DeleteBehavior.NoAction); // یا Restrict - منطق مشابه CreatedBy
            });
        }
    }
}