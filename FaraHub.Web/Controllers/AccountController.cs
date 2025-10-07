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
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ایجاد کاربر جدید (مشتری)
            var user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                IsCompanyMember = false, // مشتری جدید
                IsActive = false, // <--- تغییر: حساب جدید مشتری غیرفعال است
                CreatedAt = DateTime.UtcNow
                // DeletedAt = null; // پیش‌فرض
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // اختیاری: می‌توانید نقش پیش‌فرض "مشتری" را به کاربر جدید اختصاص دهید
                await _userManager.AddToRoleAsync(user, "Customer");

                // فقط پیام موفقیت را برمی‌گردانیم (و احتمالاً درخواست تأیید ایمیل)
                return Ok(new { message = "ثبت نام با موفقیت انجام شد. حساب شما در انتظار تأیید توسط مدیر است." });
            }

            // اگر موفق نبود، خطاها را برگردان
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }
    }
}