using System.Text;
using Application.Common.Interfaces;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class ConfigureJwt
{
    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<JwtProvider>();
        services.AddScoped<IJwtProvider>(provider => provider.GetRequiredService<JwtProvider>());
        
        // Bind JwtOptions from appsettings.json
        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));

        // Authentication and Authorization setup
        var jwtOptions = configuration.GetSection("JwtOptions").Get<JwtOptions>();
        
        // Authentication and Authorization setup
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
            });
    }
}