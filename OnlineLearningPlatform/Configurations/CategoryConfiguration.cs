using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Entities;

namespace OnlineLearningPlatform.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => c.ParentCategoryId);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Category { Id = 1, Name = "Programming", Description = "Programming and software development", ParentCategoryId = null },
            new Category { Id = 2, Name = "Backend", Description = "Server-side development", ParentCategoryId = 1 },
            new Category { Id = 3, Name = "ASP.NET", Description = "ASP.NET and .NET web development", ParentCategoryId = 2 },
            new Category { Id = 4, Name = "Frontend", Description = "Client-side development", ParentCategoryId = 1 },
            new Category { Id = 5, Name = "Data Science", Description = "Data analysis and machine learning", ParentCategoryId = null },
            new Category { Id = 6, Name = "Databases", Description = "Database design and SQL", ParentCategoryId = 2 }
        );
    }
}
