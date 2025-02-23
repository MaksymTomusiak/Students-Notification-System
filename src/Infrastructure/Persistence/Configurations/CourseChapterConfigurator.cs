using Domain.CourseChapters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourseChapterConfigurator : IEntityTypeConfiguration<CourseChapter>
{
    public void Configure(EntityTypeBuilder<CourseChapter> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new CourseChapterId(x));

        builder.Property(x => x.Name).IsRequired()
            .HasColumnType("varchar(255)");
        builder.Property(x => x.EstimatedLearningTimeMinutes).IsRequired();
        builder.Property(x => x.Number).IsRequired();
        
        builder.HasOne(x => x.Course)
            .WithMany(x => x.Chapters)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}