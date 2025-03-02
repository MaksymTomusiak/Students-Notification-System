using Amazon.Runtime;
using Amazon.S3;
using Application.Common.Interfaces.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class ConfigureServices
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddHangfire(services, configuration);

        AddNotifications(services);

        AddFileStorage(services, configuration);
    }

    private static void AddFileStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFileStorageService, S3FileStorageService>();
        var awsOptions = configuration.GetAWSOptions();
        awsOptions.Credentials = new BasicAWSCredentials(
            configuration["AWS:AccessKeyId"],
            configuration["AWS:SecretAccessKey"]);

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();
    }

    private static void AddNotifications(IServiceCollection services)
    {
        services.AddControllersWithViews()
            .AddRazorRuntimeCompilation(); 

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new InfrastructureViewLocationExpander());
        });

        services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
        services.AddSingleton<ICompositeViewEngine, CompositeViewEngine>();
        services.AddScoped<IEmailViewRenderer, EmailViewRenderer>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICourseNotificationService, CourseNotificationService>();
    }

    private static void AddHangfire(IServiceCollection services, IConfiguration configuration)
    {
        var isTesting = configuration.GetValue<bool>("IsTesting");
        if (!isTesting)
        {
            var connectionString = configuration.GetConnectionString("HangfireConnection");
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(connectionString));
            services.AddHangfireServer();
        }
    }
}