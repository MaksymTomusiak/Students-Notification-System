using Domain.Courses;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourseConfigurator : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new CourseId(x));
        
        builder.Property(x => x.Name).IsRequired()
            .HasColumnType("varchar(255)");
        builder.Property(x => x.Description).IsRequired()
            .HasColumnType("varchar(2000)");
        builder.Property(x => x.ImageUrl).IsRequired()
            .HasColumnType("varchar(300)");
        builder.Property(x => x.StartDate)
            .HasConversion(new DateTimeUtcConverter());
        builder.Property(x => x.FinishDate)
            .HasConversion(new DateTimeUtcConverter());

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatorId)
            .HasConstraintName("fk_course_users_id")
            .OnDelete(DeleteBehavior.Restrict);;
        
        builder.HasMany(x => x.CourseCategories)
            .WithOne(cc => cc.Course)
            .HasForeignKey(cc => cc.CourseId);
    }
}