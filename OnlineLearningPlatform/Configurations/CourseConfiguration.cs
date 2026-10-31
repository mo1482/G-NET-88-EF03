using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Entities;

namespace OnlineLearningPlatform.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses", table =>
        {
            table.HasCheckConstraint(
                "CK_Courses_Price_NonNegative",
                "[Price] >= 0");
            table.HasCheckConstraint(
                "CK_Courses_PublishedDate",
                "([IsPublished] = 0 AND [PublishedAt] IS NULL) OR ([IsPublished] = 1 AND [PublishedAt] IS NOT NULL)");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(c => c.Level)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.IsPublished)
            .IsRequired();

        builder.Property(c => c.PublishedAt)
            .HasColumnType("datetime2");

        builder.HasIndex(c => c.InstructorId);
        builder.HasIndex(c => c.CategoryId);

        builder.HasOne(c => c.Instructor)
            .WithMany(i => i.Courses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Category)
            .WithMany(ca => ca.Courses)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Course
            {
                Id = 1,
                Title = "C# Fundamentals",
                Description = "Learn the fundamentals of C# programming.",
                Price = 49.99m,
                Level = CourseLevel.Beginner,
                IsPublished = true,
                PublishedAt = new DateTime(2025, 4, 1),
                InstructorId = 1,
                CategoryId = 3
            },
            new Course
            {
                Id = 2,
                Title = "ASP.NET Core Web API",
                Description = "Build REST APIs using ASP.NET Core.",
                Price = 89.99m,
                Level = CourseLevel.Intermediate,
                IsPublished = true,
                PublishedAt = new DateTime(2025, 5, 1),
                InstructorId = 1,
                CategoryId = 3
            },
            new Course
            {
                Id = 3,
                Title = "HTML and CSS Foundations",
                Description = "Create responsive web page layouts.",
                Price = 29.99m,
                Level = CourseLevel.Beginner,
                IsPublished = true,
                PublishedAt = new DateTime(2025, 5, 15),
                InstructorId = 2,
                CategoryId = 4
            },
            new Course
            {
                Id = 4,
                Title = "Python for Data Analysis",
                Description = "Analyze data using Python.",
                Price = 99.99m,
                Level = CourseLevel.Intermediate,
                IsPublished = true,
                PublishedAt = new DateTime(2025, 6, 1),
                InstructorId = 3,
                CategoryId = 5
            },
            new Course
            {
                Id = 5,
                Title = "SQL Server Essentials",
                Description = "An introduction to SQL Server and relational databases.",
                Price = 59.99m,
                Level = CourseLevel.Beginner,
                IsPublished = false,
                PublishedAt = null,
                InstructorId = 2,
                CategoryId = 6
            },
            new Course
            {
                Id = 6,
                Title = "Advanced Entity Framework Core",
                Description = "Advanced EF Core queries, tracking and performance.",
                Price = 119.99m,
                Level = CourseLevel.Advanced,
                IsPublished = false,
                PublishedAt = null,
                InstructorId = 1,
                CategoryId = 3
            }
        );
    }
}
