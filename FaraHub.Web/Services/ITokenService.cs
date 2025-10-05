using FaraHub.Web.Models;

namespace FaraHub.Web.Services
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user);
    }
}