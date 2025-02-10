using Application.Common.Interfaces.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class ConfigureServices
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HangfireConnection");
        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(connectionString));
        services.AddHangfireServer();

        services.AddScoped<EmailService>();
        services.AddScoped<IEmailService>(provider => provider.GetRequiredService<EmailService>());

        services.AddScoped<CourseNotificationService>();
        services.AddScoped<ICourseNotificationService>(provider => provider.GetRequiredService<CourseNotificationService>());
    }
}