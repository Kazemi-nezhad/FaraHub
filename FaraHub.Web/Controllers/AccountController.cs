using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FaraHub.Web.Models;
using FaraHub.Web.Services; // برای ITokenService
using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        // private readonly SignInManager<AppUser> _signInManager; // دیگر نیازی نیست
        private readonly ITokenService _tokenService; // اضافه شد

        public AccountController(UserManager<AppUser> userManager, /*SignInManager<AppUser> signInManager,*/ ITokenService tokenService) // تغییر در سازنده
        {
            _userManager = userManager;
            // _signInManager = signInManager; // دیگر نیازی نیست
            _tokenService = tokenService; // اضافه شد
        }

        // POST: api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (!user.IsActive || user.DeletedAt.HasValue) // چک کردن soft delete / فعال بودن
                {
                    return Unauthorized("حساب کاربری غیرفعال یا حذف شده است.");
                }

                // تولید JWT Token
                var token = _tokenService.GenerateToken(user);

                // برگرداندن توکن و اطلاعات کاربر
                var userInfo = new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsCompanyMember = user.IsCompanyMember
                };

                return Ok(new { message = "ورود موفقیت‌آمیز", token = token, user = userInfo });
            }

            return Unauthorized("نام کاربری یا رمز عبور اشتباه است.");
        }

        // POST: api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ایجاد کاربر جدید (مشتری)
            var user = new AppUser { UserName = model.Username, Email = model.Email, FullName = model.FullName, IsCompanyMember = false /* مشتری جدید */ };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // اختیاری: می‌توانید نقش پیش‌فرض "مشتری" را به کاربر جدید اختصاص دهید
                await _userManager.AddToRoleAsync(user, "Customer"); // اطمینان حاصل کنید که نقش "Customer" وجود دارد

                // تولید JWT Token برای کاربر ثبت‌نام‌کرده (اختیاری در ثبت نام، ممکن است فقط بعد از تأیید ایمیل انجام شود)
                // var token = _tokenService.GenerateToken(user);

                // فقط پیام موفقیت را برمی‌گردانیم (و احتمالاً درخواست تأیید ایمیل)
                return Ok(new { message = "ثبت نام با موفقیت انجام شد. لطفاً ایمیل خود را برای تأیید حساب چک کنید." });
                // یا اگر می‌خواهید توکن را هم بدهید:
                // return Ok(new { message = "ثبت نام با موفقیت انجام شد.", token = token, user = userInfo });
            }

            // اگر موفق نبود، خطاها را برگردان
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }
    }

    // مدل‌های DTO برای ورود و ثبت نام (همان قبلی)
    public class LoginModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // public bool RememberMe { get; set; } = false; // دیگر برای JWT لازم نیست
    }

    public class RegisterModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}