using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Roles;
using Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class Seeder
{
    public static async Task SeedRoles(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var roleslist = await context.Roles.ToListAsync();
        if (!context.Roles.Any())
        {
            var roles = new[]
            {
                "Admin",
                "User"
            };

            foreach (var role in roles)
                await roleManager.CreateAsync(new Role { Name = role });

            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedCategories(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Categories.Any())
        {
            var categories = new[]
            {
                new { Name = "Technology" },
                new { Name = "Art" },
                new { Name = "Science" },
                new { Name = "Ai" },
                new { Name = "Education" }
            };

            foreach (var category in categories)
                await context.Categories.AddAsync(Category.New(CategoryId.New(), category.Name));

            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedUsers(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var users = await context.Users.ToListAsync();
        if (!context.Users.Any())
        {
            var roles = await context.Roles.ToListAsync();
            var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
            var userRole = roles.FirstOrDefault(r => r.Name == "User");

            if (adminRole == null || userRole == null)
            {
                throw new Exception("Roles must be seeded before adding users.");
            }

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "adminUser",
                Email = "admin@example.com"
            };
            var usualUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "usualUser",
                Email = "user@example.com"
            };

            await userManager.CreateAsync(adminUser, "AdminPass123!");
            await userManager.AddToRoleAsync(adminUser, adminRole.Name);

            await userManager.CreateAsync(usualUser, "UserPass123!");
            await userManager.AddToRoleAsync(usualUser, userRole.Name);
        }
    }

    public static async Task SeedCourses(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Courses.Any())
        {
            var categories = await context.Categories.ToListAsync();
            var users = await context.Users.ToListAsync();
            var creator = users.FirstOrDefault(u => u.UserName == "adminUser");

            if (!categories.Any() || creator == null)
            {
                throw new Exception("Ensure that categories and users are seeded before adding courses.");
            }

            var random = new Random();

            var courses = new[]
            {
                Course.New(
                    CourseId.New(),
                    "Introduction to AI",
                    //"https://example.com/ai-course.jpg",
                    "https://media.istockphoto.com/id/1055079680/vector/black-linear-photo-camera-like-no-image-available.jpg?s=612x612&w=0&k=20&c=P1DebpeMIAtXj_ZbVsKVvg-duuL0v9DlrOZUvPG6UJk=",
                    "Learn the fundamentals of AI and machine learning.",
                    creator.Id,
                    DateTime.UtcNow.AddDays(7),
                    DateTime.UtcNow.AddMonths(1),
                    "English",
                    "Basic knowledge of programming is required."),
                Course.New(
                    CourseId.New(),
                    "Advanced Programming with C#",
                    //"https://example.com/csharp-course.jpg",
                    "https://media.istockphoto.com/id/1055079680/vector/black-linear-photo-camera-like-no-image-available.jpg?s=612x612&w=0&k=20&c=P1DebpeMIAtXj_ZbVsKVvg-duuL0v9DlrOZUvPG6UJk=",
                    "Deep dive into C# and .NET best practices.",
                    creator.Id,
                    DateTime.UtcNow.AddDays(14),
                    DateTime.UtcNow.AddMonths(2),
                    "English",
                    "Basic knowledge of programming is required."),
                Course.New(
                    CourseId.New(),
                    "Cybersecurity Essentials",
                    //"https://example.com/cybersecurity-course.jpg",
                    "https://media.istockphoto.com/id/1055079680/vector/black-linear-photo-camera-like-no-image-available.jpg?s=612x612&w=0&k=20&c=P1DebpeMIAtXj_ZbVsKVvg-duuL0v9DlrOZUvPG6UJk=",
                    "Understand cybersecurity principles and best practices.",
                    creator.Id,
                    DateTime.UtcNow.AddDays(10),
                     DateTime.UtcNow.AddMonths(1),
                    "English",
                    "Basic knowledge of programming is required.")
            };

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();

            // Assign random existing categories to courses
            var courseCategories = courses.Select(course =>
                CourseCategory.New(CourseCategoryId.New(), course.Id, categories[random.Next(categories.Count)].Id));
            await context.CourseCategories.AddRangeAsync(courseCategories);
            await context.SaveChangesAsync();
        }
    }

}