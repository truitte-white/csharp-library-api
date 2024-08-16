using LibraryAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryAPI.Middleware
{
    public class LoggedInMiddleware
    {
        public readonly RequestDelegate _next;
        public readonly ILogger<LoggedInMiddleware> _logger;
        public readonly ITokenHelper _tokenHelper;

        public LoggedInMiddleware(RequestDelegate next, ILogger<LoggedInMiddleware> logger, ITokenHelper tokenHelper)
        {
            _next = next;
            _logger = logger;
            _tokenHelper = tokenHelper;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    context.Items["LoggedIn"] = false;
                    await _next(context);
                    return;
                }

                var principal = _tokenHelper.Decode(token);
                if (principal == null)
                {
                    context.Items["LoggedIn"] = false;
                }
                else
                {
                    context.Items["LoggedIn"] = true;
                    context.Items["UserId"] = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    context.Items["Role"] = principal.FindFirst(ClaimTypes.Role)?.Value;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in logged in middleware");
                context.Items["LoggedIn"] = false;
                context.Response.StatusCode = 500;
                await _next(context);
            }
        }
    }
}
