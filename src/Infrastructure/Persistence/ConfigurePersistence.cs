using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Roles;
using Domain.Users;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Persistence;

public static class ConfigurePersistence
{
    public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSourceBuild = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Default"));
        dataSourceBuild.EnableDynamicJson();
        var dataSource = dataSourceBuild.Build();

        services.AddDbContext<ApplicationDbContext>(
            options => options
                .UseNpgsql(
                    dataSource,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)));

        services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<CategoryRepository>();
        services.AddScoped<ICategoryRepository>(provider => provider.GetRequiredService<CategoryRepository>());
        services.AddScoped<ICategoryQueries>(provider => provider.GetRequiredService<CategoryRepository>());
        
        services.AddScoped<CourseRepository>();
        services.AddScoped<ICourseRepository>(provider => provider.GetRequiredService<CourseRepository>());
        services.AddScoped<ICourseQueries>(provider => provider.GetRequiredService<CourseRepository>());
        
        services.AddScoped<CourseCategoryRepository>();
        services.AddScoped<ICourseCategoryRepository>(provider => provider.GetRequiredService<CourseCategoryRepository>());
        services.AddScoped<ICourseCategoryQueries>(provider => provider.GetRequiredService<CourseCategoryRepository>());
        
        services.AddScoped<FeedbackRepository>();
        services.AddScoped<IFeedbackRepository>(provider => provider.GetRequiredService<FeedbackRepository>());
        services.AddScoped<IFeedbackQueries>(provider => provider.GetRequiredService<FeedbackRepository>());
        
        services.AddScoped<RegisterRepository>();
        services.AddScoped<IRegisterRepository>(provider => provider.GetRequiredService<RegisterRepository>());
        services.AddScoped<IRegisterQueries>(provider => provider.GetRequiredService<RegisterRepository>());
        
        services.AddScoped<CourseBanRepository>();
        services.AddScoped<ICourseBanRepository>(provider => provider.GetRequiredService<CourseBanRepository>());
        services.AddScoped<ICourseBanQueries>(provider => provider.GetRequiredService<CourseBanRepository>());

        services.AddScoped<CourseChapterRepository>();
        services.AddScoped<ICourseChapterRepository>(provider => provider.GetRequiredService<CourseChapterRepository>());
        services.AddScoped<ICourseChapterQueries>(provider => provider.GetRequiredService<CourseChapterRepository>());
        
        services.AddScoped<CourseSubChapterRepository>();
        services.AddScoped<ICourseSubChapterRepository>(provider => provider.GetRequiredService<CourseSubChapterRepository>());
        services.AddScoped<ICourseSubChapterQueries>(provider => provider.GetRequiredService<CourseSubChapterRepository>());
    }
}