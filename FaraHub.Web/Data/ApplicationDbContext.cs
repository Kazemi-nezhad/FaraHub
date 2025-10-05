using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // تغییر از Microsoft.EntityFrameworkCore
using Microsoft.EntityFrameworkCore;
using FaraHub.Web.Models; // برای AppUser

namespace FaraHub.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser> // تغییر از DbContext به IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets دیگر شما اینجا قرار می‌گیرند
        // public DbSet<AppInfo> AppInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // می‌توانید تنظیمات بیشتری برای مدل‌های Identity یا مدل‌های خود اضافه کنید
            // مثلاً تنظیم soft delete برای AppUser
            builder.Entity<AppUser>(entity =>
            {
                entity.HasQueryFilter(e => e.DeletedAt == null); // Soft Delete
            });
        }
    }
}