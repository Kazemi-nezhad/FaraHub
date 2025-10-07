// Controllers/UserController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FaraHub.Web.Data;
using FaraHub.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: api/user
        [HttpGet]
        [Authorize(Roles = "Admin,TechnicalManager")]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetUsers(
            [FromQuery] bool? isCompanyMember = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _userManager.Users.AsQueryable();

            if (isCompanyMember.HasValue)
            {
                query = query.Where(u => u.IsCompanyMember == isCompanyMember.Value);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search) || u.FullName.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = users.Select(u => new UserSummaryDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                IsCompanyMember = u.IsCompanyMember,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                DeletedAt = u.DeletedAt
            }).ToList();

            return Ok(new { Users = userDtos, TotalCount = totalCount, Page = page, PageSize = pageSize });
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,TechnicalManager")]
        public async Task<ActionResult<UserDetailDto>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("کاربر مورد نظر یافت نشد.");
            }

            if (user.DeletedAt.HasValue)
            {
                return NotFound("کاربر مورد نظر حذف شده است.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDetailDto = new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsCompanyMember = user.IsCompanyMember,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                DeletedAt = user.DeletedAt,
                Roles = roles.ToList()
            };

            return Ok(userDetailDto);
        }

        // POST: api/user
        [HttpPost]
        [Authorize(Roles = "Admin,TechnicalManager")]
        public async Task<ActionResult<UserSummaryDto>> CreateUser([FromBody] CreateUpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("رمز عبور برای کاربر جدید الزامی است.");
            }

            var user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                IsCompanyMember = model.IsCompanyMember,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (model.IsCompanyMember && model.Roles != null && model.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, model.Roles);
                    if (!roleResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(user); // حذف کاربر در صورت عدم موفقیت در اختصاص نقش
                        var roleErrors = roleResult.Errors.Select(e => e.Description); // <-- نام متغیر تغییر کرد
                        return BadRequest(new { errors = roleErrors });
                    }
                }
                else if (!model.IsCompanyMember)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                var userSummaryDto = new UserSummaryDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsCompanyMember = user.IsCompanyMember,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    DeletedAt = user.DeletedAt
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userSummaryDto);
            }

            var creationErrors = result.Errors.Select(e => e.Description); // <-- نام متغیر تغییر کرد
            return BadRequest(new { errors = creationErrors });
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,TechnicalManager")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] CreateUpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("کاربر مورد نظر یافت نشد.");
            }

            if (user.DeletedAt.HasValue)
            {
                return NotFound("کاربر مورد نظر حذف شده است.");
            }

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                return BadRequest("شما نمی‌توانید حساب خود را ویرایش کنید.");
            }

            user.UserName = model.Username;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.IsCompanyMember = model.IsCompanyMember;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            var updateErrors = result.Errors.Select(e => e.Description); // <-- نام متغیر تغییر کرد
            return BadRequest(new { errors = updateErrors });
        }

        // PUT: api/user/{id}/roles
        [HttpPut("{id}/roles")]
        [Authorize(Roles = "Admin,TechnicalManager")]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] List<string> newRoles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("کاربر مورد نظر یافت نشد.");
            }

            if (user.DeletedAt.HasValue)
            {
                return NotFound("کاربر مورد نظر حذف شده است.");
            }

            foreach (var roleName in newRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    return BadRequest($"نقش '{roleName}' وجود ندارد.");
                }
            }

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                // منطق جلوگیری از تغییر نقش خود مدیر (اختیاری)
                // if (currentRoles.Contains("Admin") && !newRoles.Contains("Admin")) {
                //     return BadRequest("شما نمی‌توانید نقش ادمین خود را حذف کنید.");
                // }
            }

            var currentUserRoles = await _userManager.GetRolesAsync(user); // <-- نام متغیر تغییر کرد
            var rolesToRemove = currentUserRoles.Except(newRoles).ToList();
            var rolesToAdd = newRoles.Except(currentUserRoles).ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var removeErrors = removeResult.Errors.Select(e => e.Description); // <-- نام متغیر تغییر کرد
                    return BadRequest(new { errors = removeErrors.Prepend("خطا در حذف نقش‌ها: ") });
                }
            }

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var addErrors = addResult.Errors.Select(e => e.Description); // <-- نام متغیر تغییر کرد
                    return BadRequest(new { errors = addErrors.Prepend("خطا در افزودن نقش‌ها: ") });
                }
            }

            return NoContent();
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,TechnicalManager")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("کاربر مورد نظر یافت نشد.");
            }

            if (user.DeletedAt.HasValue)
            {
                return NotFound("کاربر مورد نظر قبلاً حذف شده است.");
            }

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                return BadRequest("شما نمی‌توانید حساب خود را حذف کنید.");
            }

            user.DeletedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            var deleteErrors = result.Errors.Select(e => e.Description); // <-- نام متغیر تغییر کرد
            return BadRequest(new { errors = deleteErrors });
        }

        // POST: api/user/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Admin,TechnicalManager")] // فقط مدیران
        public async Task<IActionResult> ResetUserPassword(string id, [FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("کاربر مورد نظر یافت نشد.");
            }

            // چک کردن اینکه آیا کاربر حذف نشده باشد (اگر soft delete filter فعال باشد، پیدا نمی‌شود، اما برای احتیاط)
            if (user.DeletedAt.HasValue)
            {
                return NotFound("کاربر مورد نظر حذف شده است.");
            }

            // چک کردن اینکه آیا کاربر خود مدیر فعلی است (اختیاری)
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                // ممکن است بخواهید این کار را اجازه دهید یا ندهید
                // return BadRequest("شما نمی‌توانید رمز عبور خود را بازنشانی کنید.");
            }

            // گرفتن توکن بازنشانی رمز (مثلاً از طریق یک سرویس ایمیلی در آینده، یا مستقیماً در اینجا)
            // برای سادگی در اینجا، یک توکن تولید نمی‌کنیم و فقط رمز جدید را ست می‌کنیم
            // اما روش استاندارد این است که یک لینک با توکن به کاربر ارسال شود.
            // برای مدیر، ممکن است بخواهید فقط رمز جدید را وارد کند و بلافاصله تغییر کند.

            var token = await _userManager.GeneratePasswordResetTokenAsync(user); // این توکن معمولاً برای ایمیل ارسال می‌شود
            // اما در اینجا، ما مستقیماً از رمز جدید استفاده می‌کنیم
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                return NoContent(); // یا می‌توانید پیام موفقیت را برگردانید
            }

            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        // PUT: api/user/{id}/confirm (برای فعال کردن حساب مشتری)
        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin,TechnicalManager")] // فقط مدیران فنی/ارشد
        public async Task<IActionResult> ConfirmUserAccount(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("کاربر مورد نظر یافت نشد.");
            }

            // چک کردن اینکه آیا کاربر حذف نشده باشد
            if (user.DeletedAt.HasValue)
            {
                return NotFound("کاربر مورد نظر حذف شده است.");
            }

            // چک کردن اینکه آیا کاربر مشتری است (اختیاری اما منطقی)
            if (user.IsCompanyMember)
            {
                return BadRequest("این عملیات فقط برای حساب‌های مشتری قابل انجام است.");
            }

            // چک کردن اینکه آیا حساب قبلاً فعال شده است (اختیاری)
            if (user.IsActive)
            {
                return BadRequest("حساب کاربر قبلاً فعال شده است.");
            }

            // فعال کردن حساب (مثلاً با تغییر IsActive یا تأیید ایمیل)
            // در اینجا، IsActive را true می‌کنیم
            user.IsActive = true;
            // همچنین می‌توانید DeletedAt را null کنید اگر قبلاً حذف شده بوده (مثلاً به عنوان یک "غیرفعال" نگه داشته شده)
            // user.DeletedAt = null; // اگر DeletedAt برای "غیرفعال" بودن استفاده می‌شود، این خط معنا دارد. اما اگر فقط IsActive است، این لازم نیست.
            // اگر از EmailToken برای تأیید استفاده می‌کردید، باید VerifiedEmail را true کنید:
            // user.EmailConfirmed = true;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }
    }
}