using System.Reflection;
using Domain.Categories;
using Domain.CourseBans;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Feedbacks;
using Domain.Registers;
using Domain.Roles;
using Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Register> Registers { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<CourseCategory> CourseCategories { get; set; }
    public DbSet<CourseBan> CourseBans { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}