using Amazon.Runtime;
using Amazon.S3;
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
        
        var isTesting = configuration.GetValue<bool>("IsTesting");
        if (!isTesting)
        {
            var connectionString = configuration.GetConnectionString("HangfireConnection");
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(connectionString));
            services.AddHangfireServer();
        }
        
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICourseNotificationService, CourseNotificationService>();
        
        services.AddSingleton<IFileStorageService, S3FileStorageService>();
        var awsOptions = configuration.GetAWSOptions();
        awsOptions.Credentials = new BasicAWSCredentials(
            configuration["AWS:AccessKeyId"],
            configuration["AWS:SecretAccessKey"]);

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();
    }
}