﻿// Controllers/MessageController.cs
using FaraHub.Web.Data;
using FaraHub.Web.DTOs;
using FaraHub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static FaraHub.Web.Controllers.TicketController;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment; // برای دسترسی به wwwroot

        public MessageController(ApplicationDbContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: api/message/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<MessageDto>> GetMessage(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null || !currentUser.IsActive || currentUser.DeletedAt.HasValue)
                return Unauthorized();

            var message = await _context.Messages
                .Where(m => m.Id == id && m.DeletedAt == null)
                .Include(m => m.SentBy)
                .Include(m => m.Attachments.Where(a => a.DeletedAt == null))
                .FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            // چک کردن دسترسی به تیکت مرتبط
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == message.TicketId);
            if (ticket == null)
            {
                return NotFound(); // یا Unauthorized اگر تیکت وجود نداشته باشد
            }

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

            var messageDto = new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                SentBy = new UserSummaryDto { Id = message.SentBy.Id, UserName = message.SentBy.UserName, FullName = message.SentBy.FullName },
                Attachments = message.Attachments?.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    Size = a.Size,
                    ContentType = a.ContentType,
                    DownloadUrl = Url.Action("Download", "Attachment", new { id = a.Id }, Request.Scheme) ?? string.Empty // یا یک کنترلر جداگانه
                }).ToList()
            };

            return Ok(messageDto);
        }

        // POST: api/message/send/{ticketId}
        [HttpPost("send/{ticketId}")]
        [Authorize]
        public async Task<ActionResult<MessageDto>> SendMessage(int ticketId, [FromForm] SendMessageDto model) // FromForm برای فایل‌ها
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

            // چک کردن دسترسی به تیکت
            Ticket ticket;
            if (currentUser.IsCompanyMember)
            {
                ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId && (t.CreatedById == currentUserId || (t.AssignedToId != null && t.AssignedToId == currentUserId)));
            }
            else
            {
                ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId && (t.CreatedById == currentUserId || (t.CustomerId != null && t.CustomerId == currentUserId)));
            }

            if (ticket == null)
            {
                return NotFound("تیکت مورد نظر یافت نشد یا دسترسی ندارید.");
            }

            // اعتبارسنجی فایل‌ها
            if (model.Files != null && model.Files.Count > 5)
            {
                return BadRequest("حداکثر 5 فایل قابل آپلود است.");
            }

            long totalSize = 0;
            foreach (var file in model.Files ?? new List<IFormFile>())
            {
                if (file.Length == 0) continue;
                totalSize += file.Length;
                if (totalSize > 50 * 1024 * 1024) // 50 مگابایت
                {
                    return BadRequest("حجم کل فایل‌ها نباید بیشتر از 50 مگابایت باشد.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest($"فرمت فایل {file.FileName} مجاز نیست.");
                }
            }

            // ایجاد پیام
            var message = new Message
            {
                Content = model.Content,
                SentById = currentUserId,
                TicketId = ticketId,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync(); // ابتدا پیام ذخیره شود تا Id داشته باشیم

            // آپلود فایل‌ها و ذخیره در دیتابیس
            if (model.Files != null)
            {
                var ticketUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "tickets", ticketId.ToString(), "messages", message.Id.ToString());
                Directory.CreateDirectory(ticketUploadPath); // ایجاد پوشه اگر وجود نداشته باشد

                foreach (var file in model.Files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(ticketUploadPath, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var attachment = new Attachment
                        {
                            FileName = file.FileName,
                            FilePath = Path.Combine("uploads", "tickets", ticketId.ToString(), "messages", message.Id.ToString(), fileName), // مسیر نسبی
                            Size = file.Length,
                            ContentType = file.ContentType,
                            MessageId = message.Id
                        };

                        _context.Attachments.Add(attachment);
                    }
                }
                await _context.SaveChangesAsync(); // ذخیره فایل‌های پیوست
            }

            // تغییر وضعیت تیکت اگر فرستنده مشتری بود
            if (!currentUser.IsCompanyMember) // اگر مشتری بود
            {
                ticket.Status = TicketStatus.CustomerReplied;
                ticket.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(); // ذخیره تغییرات تیکت
            }

            // بازگرداندن پیام ایجاد شده
            var messageDto = new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                SentBy = new UserSummaryDto { Id = message.SentBy.Id, UserName = message.SentBy.UserName, FullName = message.SentBy.FullName },
                Attachments = (await _context.Attachments.Where(a => a.MessageId == message.Id).ToListAsync())
                    .Select(a => new AttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        Size = a.Size,
                        ContentType = a.ContentType,
                        DownloadUrl = Url.Action("Download", "Attachment", new { id = a.Id }, Request.Scheme) ?? string.Empty // یا یک کنترلر جداگانه
                    }).ToList()
            };

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, messageDto);
        }

        // GET: api/message/ticket/{ticketId}
        [HttpGet("ticket/{ticketId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForTicket(int ticketId)
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
                    .FirstOrDefaultAsync(t => t.Id == ticketId && (t.CreatedById == currentUserId || (t.AssignedToId != null && t.AssignedToId == currentUserId)));
            }
            else
            {
                ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId && (t.CreatedById == currentUserId || (t.CustomerId != null && t.CustomerId == currentUserId)));
            }

            if (ticket == null)
            {
                return NotFound("تیکت مورد نظر یافت نشد یا دسترسی ندارید.");
            }

            var messages = await _context.Messages
                .Where(m => m.TicketId == ticketId && m.DeletedAt == null)
                .Include(m => m.SentBy)
                .Include(m => m.Attachments.Where(a => a.DeletedAt == null))
                .OrderBy(m => m.SentAt) // مرتب‌سازی بر اساس زمان ارسال
                .ToListAsync();

            var messageDtos = messages.Select(m => new MessageDto
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
                    DownloadUrl = Url.Action("Download", "Attachment", new { id = a.Id }, Request.Scheme) ?? string.Empty // یا یک کنترلر جداگانه
                }).ToList()
            }).ToList();

            return Ok(messageDtos);
        }
    }
}