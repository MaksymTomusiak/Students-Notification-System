using Domain.Feedbacks;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FeedbackConfigurator : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new FeedbackId(x));
        builder.Property(x => x.Content).IsRequired()
            .HasColumnType("varchar(300)");
        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasConversion(new DateTimeUtcConverter());

        builder.HasOne(x => x.Course)
            .WithMany(x => x.Feedbacks)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.Feedbacks)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}