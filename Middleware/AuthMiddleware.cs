using LibraryAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryAPI.Middleware
{
    public class AuthMiddleware
    {
        public readonly RequestDelegate _next;
        public readonly ILogger<AuthMiddleware> _logger;
        public readonly ITokenHelper _tokenHelper;

        public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger, ITokenHelper tokenHelper)
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
                    throw new Exception("Token not found");
                }

                var principal = _tokenHelper.Decode(token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("Invalid token: userId not found");
                }

                context.Items["UserId"] = userId; // Store userId in context items
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in authentication middleware");
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.Redirect("/user/login"); // Redirect to login page
            }
        }
    }
}
