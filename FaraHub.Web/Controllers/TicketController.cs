// TicketController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FaraHub.Web.Data;
using FaraHub.Web.Models;
using FaraHub.Web.Services; // برای TokenService اگر لازم باشد
using System.ComponentModel.DataAnnotations;
using FaraHub.Web.DTOs;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TicketController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/ticket/my-tickets
        [HttpGet("my-tickets")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetMyTickets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? search = null)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            IQueryable<Ticket> query;

            if (currentUser.IsCompanyMember)
            {
                // اعضا شرکت: تیکت‌های ایجاد شده توسط خود + تیکت‌های ارجاع‌شده به خود
                query = _context.Tickets
                    .Where(t => t.CreatedById == currentUserId || t.AssignedToId == currentUserId);
            }
            else
            {
                // مشتریان: تیکت‌های خود + تیکت‌هایی که به عنوان مشتری (Customer) اختصاص داده شده
                query = _context.Tickets
                    .Where(t => t.CreatedById == currentUserId || t.CustomerId == currentUserId);
            }

            // اعمال فیلترها
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatus>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }
            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TicketPriority>(priority, true, out var priorityEnum))
            {
                query = query.Where(t => t.Priority == priorityEnum);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var tickets = await query
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Customer)
                .OrderByDescending(t => t.CreatedAt) // مرتب‌سازی
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var ticketDtos = tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                LastActivityAt = t.LastActivityAt,
                CreatedBy = new UserSummaryDto { Id = t.CreatedBy.Id, UserName = t.CreatedBy.UserName, FullName = t.CreatedBy.FullName },
                AssignedTo = t.AssignedTo != null ? new UserSummaryDto { Id = t.AssignedTo.Id, UserName = t.AssignedTo.UserName, FullName = t.AssignedTo.FullName } : null,
                Customer = t.Customer != null ? new UserSummaryDto { Id = t.Customer.Id, UserName = t.Customer.UserName, FullName = t.Customer.FullName } : null
            }).ToList();

            return Ok(new { Tickets = ticketDtos, TotalCount = totalCount, Page = page, PageSize = pageSize });
        }

        // GET: api/ticket/all (فقط برای مدیران ارشد)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")] // تغییر دهید بسته به نقش‌های دقیق مدیریت کل
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAllTickets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? search = null)
        {
            // مشابه بالا، اما بدون فیلتر کاربر
            IQueryable<Ticket> query = _context.Tickets;

            // اعمال فیلترها
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatus>(status, true, out var statusEnum))
            {
                query = query.Where(t => t.Status == statusEnum);
            }
            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TicketPriority>(priority, true, out var priorityEnum))
            {
                query = query.Where(t => t.Priority == priorityEnum);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var tickets = await query
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Customer)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var ticketDtos = tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                LastActivityAt = t.LastActivityAt,
                CreatedBy = new UserSummaryDto { Id = t.CreatedBy.Id, UserName = t.CreatedBy.UserName, FullName = t.CreatedBy.FullName },
                AssignedTo = t.AssignedTo != null ? new UserSummaryDto { Id = t.AssignedTo.Id, UserName = t.AssignedTo.UserName, FullName = t.AssignedTo.FullName } : null,
                Customer = t.Customer != null ? new UserSummaryDto { Id = t.Customer.Id, UserName = t.Customer.UserName, FullName = t.Customer.FullName } : null
            }).ToList();

            return Ok(new { Tickets = ticketDtos, TotalCount = totalCount, Page = page, PageSize = pageSize });
        }

        // GET: api/ticket/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TicketDetailDto>> GetTicket(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            Ticket ticket;
            if (currentUser.IsCompanyMember)
            {
                ticket = await _context.Tickets
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Customer)
                    .Include(t => t.Messages.Where(m => m.DeletedAt == null)) // فقط پیام‌های ناپاک
                        .ThenInclude(m => m.SentBy)
                    .Include(t => t.Messages)
                        .ThenInclude(m => m.Attachments.Where(a => a.DeletedAt == null)) // فقط فایل‌های ناپاک
                    .Include(t => t.TimeLogs) // فقط برای نمایش کل زمان
                    .FirstOrDefaultAsync(t =>
                        t.Id == id &&
                        (t.CreatedById == currentUserId || (t.AssignedToId != null && t.AssignedToId == currentUserId))
                    );
            }
            else
            {
                ticket = await _context.Tickets
                   .Include(t => t.CreatedBy)
                   .Include(t => t.AssignedTo)
                   .Include(t => t.Customer)
                   .Include(t => t.Messages.Where(m => m.DeletedAt == null))
                       .ThenInclude(m => m.SentBy)
                   .Include(t => t.Messages)
                       .ThenInclude(m => m.Attachments.Where(a => a.DeletedAt == null))
                   .Include(t => t.TimeLogs) // فقط برای نمایش کل زمان
                   .FirstOrDefaultAsync(t =>
                       t.Id == id &&
                      (t.CreatedById == currentUserId || (t.CustomerId != null && t.CustomerId == currentUserId))
                   );
            }

            if (ticket == null)
            {
                return NotFound();
            }

            // محاسبه کل زمان صرف شده
            var totalTime = TimeSpan.Zero;
            if (ticket.TimeLogs != null && ticket.TimeLogs.Any())
            {
                // ابتدا رکوردهایی که TotalDuration دارند یا EndTime دارند را فیلتر می‌کنیم
                // سپس مقدار TimeSpan را محاسبه می‌کنیم
                // سپس فقط TimeSpanهایی که null نیستند را جمع می‌زنیم
                var validDurations = ticket.TimeLogs
                    .Where(tl => tl.StartTime != default && tl.DeletedAt == null)
                    .Select(tl => tl.TotalDuration ?? (tl.EndTime.HasValue ? (TimeSpan?)(tl.EndTime.Value - tl.StartTime) : null))
                    .Where(duration => duration.HasValue) // فقط TimeSpanهای غیر null
                    .Select(duration => duration.Value); // فقط مقدار TimeSpan

                totalTime = TimeSpan.FromTicks(validDurations.Sum(ts => ts.Ticks)); // جمع تیک‌ها و تبدیل به TimeSpan
            }

            var ticketDetailDto = new TicketDetailDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                LastActivityAt = ticket.LastActivityAt,
                CreatedBy = new UserSummaryDto { Id = ticket.CreatedBy.Id, UserName = ticket.CreatedBy.UserName, FullName = ticket.CreatedBy.FullName },
                AssignedTo = ticket.AssignedTo != null ? new UserSummaryDto { Id = ticket.AssignedTo.Id, UserName = ticket.AssignedTo.UserName, FullName = ticket.AssignedTo.FullName } : null,
                Customer = ticket.Customer != null ? new UserSummaryDto { Id = ticket.Customer.Id, UserName = ticket.Customer.UserName, FullName = ticket.Customer.FullName } : null,
                Messages = ticket.Messages?.Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    SentBy = new UserSummaryDto { Id = m.SentBy.Id, UserName = m.SentBy.UserName, FullName = m.SentBy.FullName },
                    Attachments = m.Attachments?.Select(a => new AttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        Size = a.Size,
                        ContentType = a.ContentType,
                        // FilePath نباید مستقیماً ارسال شود، بلکه یک لینک دانلود ایمن ایجاد شود
                        DownloadUrl = Url.Action("Download", "Attachment", new { id = a.Id }, Request.Scheme) ?? string.Empty // یا یک کنترلر جداگانه
                    }).ToList()
                }).ToList(),
                TotalTimeSpent = totalTime
            };

            return Ok(ticketDetailDto);
        }

        // POST: api/ticket
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            string targetCustomerId = currentUserId; // پیش‌فرض: تیکت برای خود کاربر
            if (currentUser.IsCompanyMember && !string.IsNullOrEmpty(model.CustomerId))
            {
                var customer = await _userManager.FindByIdAsync(model.CustomerId);
                if (customer != null && !customer.IsCompanyMember)
                {
                    targetCustomerId = model.CustomerId;
                }
                // در غیر این صورت، targetCustomerId همچنان currentUserId است
            }

            var ticket = new Ticket
            {
                Title = model.Title,
                Description = model.Description,
                Status = TicketStatus.InProgress, // اولیه
                Priority = model.Priority,
                CreatedById = currentUserId,
                CustomerId = targetCustomerId,
                // AssignedToId فقط توسط مدیران یا بعداً از طریق API مربوطه ست می‌شود
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var ticketDto = new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                LastActivityAt = ticket.LastActivityAt,
                CreatedBy = new UserSummaryDto { Id = ticket.CreatedBy.Id, UserName = ticket.CreatedBy.UserName, FullName = ticket.CreatedBy.FullName },
                AssignedTo = ticket.AssignedTo != null ? new UserSummaryDto { Id = ticket.AssignedTo.Id, UserName = ticket.AssignedTo.UserName, FullName = ticket.AssignedTo.FullName } : null,
                Customer = ticket.Customer != null ? new UserSummaryDto { Id = ticket.Customer.Id, UserName = ticket.Customer.UserName, FullName = ticket.Customer.FullName } : null
            };

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticketDto);
        }

        // PUT: api/ticket/{id}/status (فقط برای اعضا شرکت)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "TechnicalManager,SalesManager,Accountant,Support,SEO")] // نقش‌های عضو شرکت
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null || ticket.AssignedToId != currentUserId)
            {
                return NotFound(); // یا Forbidden؟ بسته به منطق
            }

            ticket.Status = model.NewStatus;
            ticket.UpdatedAt = DateTime.UtcNow;
            // LastActivityAt ممکن است باید هنگام ارسال پیام تغییر کند، نه فقط وضعیت
            //if (model.NewStatus == TicketStatus.CustomerReplied || model.NewStatus == TicketStatus.Completed)
            //{
            //     ticket.LastActivityAt = DateTime.UtcNow;
            //}
            ticket.LastActivityAt = DateTime.UtcNow; // به عنوان مثال، هر تغییری فعالیت محسوب می‌شود

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/ticket/{id}/assign (فقط برای مدیران ارشد)
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Admin")] // تغییر دهید بسته به نقش‌های مدیریت تیکت
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(model.AssignedToId))
            {
                var assignee = await _userManager.FindByIdAsync(model.AssignedToId);
                if (assignee == null || !assignee.IsCompanyMember)
                {
                    return BadRequest("کاربر مورد نظر یک عضو شرکت نیست یا وجود ندارد.");
                }
                ticket.AssignedToId = model.AssignedToId;
            }
            else
            {
                ticket.AssignedToId = null;
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}