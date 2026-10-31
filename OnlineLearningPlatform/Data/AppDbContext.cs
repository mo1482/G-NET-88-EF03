using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Entities;

namespace OnlineLearningPlatform.Data;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string connectionString = Environment.GetEnvironmentVariable("ONLINE_LEARNING_CONNECTION")
                ?? @"Server=(localdb)\MSSQLLocalDB;Database=OnlineLearningPlatformDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
