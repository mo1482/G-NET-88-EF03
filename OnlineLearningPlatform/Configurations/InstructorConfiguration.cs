using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Entities;

namespace OnlineLearningPlatform.Configurations;

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.ToTable("Instructors");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.FullName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Bio)
            .HasMaxLength(500);

        builder.Property(i => i.JoinedAt)
            .IsRequired()
            .HasColumnType("date");

        builder.HasData(
            new Instructor
            {
                Id = 1,
                FullName = "Ahmed Hassan",
                Email = "ahmed.hassan@learnhub.com",
                Bio = "C# and .NET instructor.",
                JoinedAt = new DateTime(2025, 1, 10)
            },
            new Instructor
            {
                Id = 2,
                FullName = "Mariam Ali",
                Email = "mariam.ali@learnhub.com",
                Bio = "Frontend development instructor.",
                JoinedAt = new DateTime(2025, 2, 15)
            },
            new Instructor
            {
                Id = 3,
                FullName = "Omar Mostafa",
                Email = "omar.mostafa@learnhub.com",
                Bio = "Data and database instructor.",
                JoinedAt = new DateTime(2025, 3, 20)
            });
    }
}
