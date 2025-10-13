using FaraHub.Web.Data;
using FaraHub.Web.Models;
using FaraHub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        // POST: api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (!user.IsActive || user.DeletedAt.HasValue)
                {
                    return Unauthorized("حساب کاربری غیرفعال یا حذف شده است.");
                }

                var token = _tokenService.GenerateToken(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    // Path = "/",
                    // Domain = ".yourdomain.com",
                    MaxAge = TimeSpan.FromMinutes(60)
                };

                Response.Cookies.Append("auth_token", token, cookieOptions);

                var userInfo = new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsCompanyMember = user.IsCompanyMember
                };

                // نباید توکن در بدنه ارسال شود
                return Ok(new { message = "ورود موفقیت‌آمیز", user = userInfo });
            }

            return Unauthorized("نام کاربری یا رمز عبور اشتباه است.");
        }

        // POST: api/account/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("auth_token");

            return Ok(new { message = "خروج موفقیت‌آمیز" });
        }

        // POST: api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                IsCompanyMember = false,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");

                return Ok(new { message = "ثبت نام با موفقیت انجام شد. حساب شما در انتظار تأیید توسط مدیر است." });
            }

            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        // GET: api/account/status
        [HttpGet("status")]
        [Authorize]
        public async Task<ActionResult<object>> GetAuthStatus()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return NotFound();
            }

            var userInfo = new
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                FullName = currentUser.FullName,
                IsCompanyMember = currentUser.IsCompanyMember,
                Roles = (await _userManager.GetRolesAsync(currentUser)).ToList()
            };

            return Ok(new { user = userInfo });
        }
    }
}