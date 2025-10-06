// Controllers/AttachmentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FaraHub.Web.Data;
using FaraHub.Web.Models;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AttachmentController(ApplicationDbContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: api/attachment/download/{id}
        [HttpGet("download/{id}")]
        [Authorize] // فقط کاربر وارد شده
        public async Task<IActionResult> Download(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            // یافتن فایل پیوست
            var attachment = await _context.Attachments
                .Include(a => a.Message) // برای دسترسی به تیکت
                    .ThenInclude(m => m.Ticket)
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);

            if (attachment == null)
            {
                return NotFound("فایل پیوست یافت نشد.");
            }

            // چک کردن دسترسی به تیکت مرتبط با پیام این فایل
            var ticket = attachment.Message.Ticket;
            bool canAccess = false;
            if (currentUser.IsCompanyMember)
            {
                if (ticket.CreatedById == currentUserId || ticket.AssignedToId == currentUserId)
                    canAccess = true;
            }
            else // مشتری
            {
                if (ticket.CreatedById == currentUserId || ticket.CustomerId == currentUserId)
                    canAccess = true;
            }

            if (!canAccess)
            {
                return Forbid(); // یا 403
            }

            // ساخت مسیر کامل فایل
            var fullPath = Path.Combine(_environment.WebRootPath, attachment.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("فایل در سیستم وجود ندارد.");
            }

            // خواندن فایل
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var fileName = attachment.FileName;

            // ارسال فایل
            return File(fileBytes, attachment.ContentType, fileName);
        }
    }
}