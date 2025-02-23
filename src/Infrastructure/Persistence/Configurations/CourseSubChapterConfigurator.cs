using Domain.CourseSubChapters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourseSubChapterConfigurator : IEntityTypeConfiguration<CourseSubChapter>
{
    public void Configure(EntityTypeBuilder<CourseSubChapter> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new CourseSubChapterId(x));

        builder.Property(x => x.Name).IsRequired()
            .HasColumnType("varchar(255)");
        builder.Property(x => x.Content).IsRequired()
            .HasColumnType("varchar(2000)");
        builder.Property(x => x.EstimatedLearningTimeMinutes).IsRequired();
        builder.Property(x => x.Number).IsRequired();
        
        builder.HasOne(x => x.CourseChapter)
            .WithMany(x => x.SubChapters)
            .HasForeignKey(x => x.CourseChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}