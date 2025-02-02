using Application.Common.Interfaces;
using Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ConfigureJwt
{
    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<JwtProvider>();
        services.AddScoped<IJwtProvider>(provider => provider.GetRequiredService<JwtProvider>());
    }
}