using Domain.CourseBans;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CourseBanConfigurator : IEntityTypeConfiguration<CourseBan>
{
    public void Configure(EntityTypeBuilder<CourseBan> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new CourseBanId(x));
        builder.Property(x => x.BannedAt)
            .IsRequired()
            .HasConversion(new DateTimeUtcConverter());
        builder.HasOne(x => x.User)
            .WithMany(x => x.CourseBans)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Course)
            .WithMany(x => x.CourseBans)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.Reason).IsRequired()
            .HasColumnType("varchar(255)");
    }
}