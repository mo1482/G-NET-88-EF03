using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Entities;

namespace OnlineLearningPlatform.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments", table =>
        {
            table.HasCheckConstraint(
                "CK_Enrollments_ProgressPercentage",
                "[ProgressPercentage] >= 0 AND [ProgressPercentage] <= 100");
            table.HasCheckConstraint(
                "CK_Enrollments_FinalGrade_OnlyWhenCompleted",
                "[Status] = 2 OR [FinalGrade] IS NULL");
            table.HasCheckConstraint(
                "CK_Enrollments_StatusMatchesProgress",
                "([Status] = 2 AND [ProgressPercentage] = 100) OR ([Status] <> 2 AND [ProgressPercentage] < 100)");
        });

        builder.HasKey(e => new { e.StudentId, e.CourseId });

        builder.Property(e => e.EnrolledAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(e => e.ProgressPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.FinalGrade)
            .HasPrecision(5, 2);

        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
