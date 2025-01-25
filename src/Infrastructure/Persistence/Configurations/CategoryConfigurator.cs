using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new CategoryId(x));
        builder.Property(x => x.Name).IsRequired()
            .HasColumnType("varchar(255)");
        
        builder.HasMany(x => x.CourseCategories)
            .WithOne(cc => cc.Category)
            .HasForeignKey(cc => cc.CategoryId);
    }
}