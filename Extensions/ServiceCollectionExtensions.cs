using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryAPI.Data;
using LibraryAPI.Helpers;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LibraryAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Retrieve connection string and ensure it's not null
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string 'DefaultConnection' is not configured.", "DefaultConnection");
            }

            // Configure DbContext with MySQL
            services.AddDbContext<LibraryDbContext>(options =>
                options.UseMySQL(connectionString));

            // Register services with dependency injection
            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IBorrowerService, BorrowerService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<IUserService, UserService>();
            services.AddScoped<IDbHelper, DbHelper>();
            services.AddScoped<ITokenHelper, TokenHelper>();

            // Retrieve JWT configuration values
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];
            var jwtSecretKey = configuration["Jwt:SecretKey"];

            // Ensure JWT configuration values are not null
            if (string.IsNullOrEmpty(jwtIssuer))
            {
                throw new ArgumentNullException("Jwt:Issuer", "JWT Issuer is not configured.");
            }

            if (string.IsNullOrEmpty(jwtAudience))
            {
                throw new ArgumentNullException("Jwt:Audience", "JWT Audience is not configured.");
            }

            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                throw new ArgumentNullException("Jwt:SecretKey", "JWT Secret Key is not configured.");
            }

            // Ensure the secret key is not null
            var keyBytes = Encoding.ASCII.GetBytes(jwtSecretKey);

            // Configure JWT authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                    };
                });

            return services;
        }
    }
}
