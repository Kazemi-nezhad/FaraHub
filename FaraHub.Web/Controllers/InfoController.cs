using Microsoft.AspNetCore.Mvc;
using FaraHub.Web.Data;
using FaraHub.Web.Models;

namespace FaraHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("status")] // مسیر: GET api/info/status
        public IActionResult GetStatus()
        {
            var info = new AppInfo { Name = "FaraHub API", Version = "1.0.0" };
            return Ok(info); // خروجی JSON: {"Id": 0, "Name": "FaraHub API", "Version": "1.0.0"}
        }
    }
}