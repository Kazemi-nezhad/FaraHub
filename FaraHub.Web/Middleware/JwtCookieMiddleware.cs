using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FaraHub.Web.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("auth_token", out var token))
            {
                context.Request.Headers.Authorization = $"Bearer {token}";
            }

            await _next(context);
        }
    }
}