using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VideoPlatform.Core.Implementations.Auth;
using VideoPlatform.Core.Implementations.User;
using VideoPlatform.Core.Interfaces.Auth;
using VideoPlatform.Core.Interfaces.User;
using VideoPlatform.Core.Models.User;
using VideoPlatform.Data;

namespace VideoPlatform.API
{
    public static class Startup
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<IDbContext, DapperDbContext>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher<UserResponse>, PasswordHasher<UserResponse>>();
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtIssuer = configuration.GetSection("Jwt:Issuer").Get<string>();
            var jwtKey = configuration.GetSection("Jwt:Key").Get<string>();

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
                      ValidAudience = jwtIssuer,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                  };
              });
        }
    }
}
