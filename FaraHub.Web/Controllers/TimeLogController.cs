// Controllers/TimeLogController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FaraHub.Web.Data;
using FaraHub.Web.Models;
using System.ComponentModel.DataAnnotations;
using FaraHub.Web.DTOs;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TimeLogController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/timelog/start/{ticketId}
        [HttpPost("start/{ticketId}")]
        [Authorize] // فقط کاربر وارد شده
        public async Task<IActionResult> StartTimer(int ticketId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            // چک کردن دسترسی به تیکت (همان منطق قبلی)
            Ticket ticket;
            if (currentUser.IsCompanyMember)
            {
                ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId && t.AssignedToId == currentUserId);
            }
            else
            {
                // مشتریان معمولاً زمان کاری نمی‌کنند، اما اگر نیاز باشد، باید منطق متفاوتی داشته باشد
                // برای اینجا، فقط اعضا شرکت می‌توانند زمان بگیرند
                return Forbid(); // یا 403
            }

            if (ticket == null)
            {
                return NotFound("تیکت مورد نظر یافت نشد یا دسترسی ندارید.");
            }

            // چک کردن وجود یک زمان "در حال اجرا" قبلی برای این کاربر-تیکت
            var existingRunningLog = await _context.TimeLogs
                .FirstOrDefaultAsync(tl => tl.UserId == currentUserId && tl.TicketId == ticketId && tl.EndTime == null && tl.DeletedAt == null);

            if (existingRunningLog != null)
            {
                // ممکن است بخواهید زمان قبلی را متوقف کنید یا خطا دهید
                // برای این مثال، خطا می‌دهیم
                return BadRequest("زمانی قبلاً برای این تیکت در حال اجرا است. لطفاً آن را متوقف کنید.");
            }

            var timeLog = new TimeLog
            {
                UserId = currentUserId,
                TicketId = ticketId,
                StartTime = DateTime.UtcNow,
                EndTime = null, // هنوز متوقف نشده
                TotalDuration = null // هنوز محاسبه نشده
            };

            _context.TimeLogs.Add(timeLog);
            await _context.SaveChangesAsync();

            return Ok(new { message = "زمان شروع ثبت شد.", timeLogId = timeLog.Id });
        }

        // POST: api/timelog/stop/{timeLogId}
        [HttpPost("stop/{timeLogId}")]
        [Authorize] // فقط کاربر وارد شده
        public async Task<IActionResult> StopTimer(int timeLogId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            var timeLog = await _context.TimeLogs
                .FirstOrDefaultAsync(tl => tl.Id == timeLogId && tl.UserId == currentUserId && tl.EndTime == null && tl.DeletedAt == null);

            if (timeLog == null)
            {
                return NotFound("رکورد زمان مورد نظر یافت نشد یا قبلاً متوقف شده یا متعلق به شما نیست.");
            }

            timeLog.EndTime = DateTime.UtcNow;
            timeLog.TotalDuration = timeLog.EndTime.Value - timeLog.StartTime;

            await _context.SaveChangesAsync();

            return Ok(new { message = "زمان متوقف شد.", totalDuration = timeLog.TotalDuration });
        }

        // GET: api/timelog/ticket/{ticketId}
        [HttpGet("ticket/{ticketId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TimeLogDto>>> GetTimeLogsForTicket(int ticketId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            // فقط مدیران یا کاربر مسئول تیکت می‌تواند زمان‌ها را ببیند
            bool canAccess = false;
            if (await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                canAccess = true;
            }
            else if (currentUser.IsCompanyMember)
            {
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId && t.AssignedToId == currentUserId);
                if (ticket != null) canAccess = true;
            }

            if (!canAccess)
            {
                return Forbid(); // یا 403
            }

            var timeLogs = await _context.TimeLogs
                .Where(tl => tl.TicketId == ticketId && tl.DeletedAt == null)
                .Include(tl => tl.User) // برای نمایش نام کاربر
                .OrderByDescending(tl => tl.StartTime) // مرتب‌سازی
                .ToListAsync();

            var timeLogDtos = timeLogs.Select(tl => new TimeLogDto
            {
                Id = tl.Id,
                UserId = tl.UserId,
                UserName = tl.User.UserName, // یا FullName
                TicketId = tl.TicketId,
                StartTime = tl.StartTime,
                EndTime = tl.EndTime,
                TotalDuration = tl.TotalDuration
            }).ToList();

            return Ok(timeLogDtos);
        }
    }
}