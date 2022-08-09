using JCTO.Domain.Dtos;
using System.Security.Claims;

namespace JCTO.Api.Middlewares
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserContext userContext)
        {
            var nameIdentifier = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            userContext.UserId = nameIdentifier?.Value;

            await _next(context);
        }
    }

    public static class UserContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserContext(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserContextMiddleware>();
        }
    }
}
