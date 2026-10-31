using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OnlineLearningPlatform.Data;

#nullable disable

namespace OnlineLearningPlatform.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610310002_SeedReferenceData")]
public partial class SeedReferenceData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Instructors",
            columns: new[] { "Id", "Bio", "Email", "FullName", "JoinedAt" },
            values: new object[,]
            {
                { 1, "C# and .NET instructor.", "ahmed.hassan@learnhub.com", "Ahmed Hassan", new DateTime(2025, 1, 10) },
                { 2, "Frontend development instructor.", "mariam.ali@learnhub.com", "Mariam Ali", new DateTime(2025, 2, 15) },
                { 3, "Data and database instructor.", "omar.mostafa@learnhub.com", "Omar Mostafa", new DateTime(2025, 3, 20) }
            });

        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Id", "Description", "Name", "ParentCategoryId" },
            values: new object[,]
            {
                { 1, "Programming and software development", "Programming", null },
                { 2, "Server-side development", "Backend", 1 },
                { 3, "ASP.NET and .NET web development", "ASP.NET", 2 },
                { 4, "Client-side development", "Frontend", 1 },
                { 5, "Data analysis and machine learning", "Data Science", null },
                { 6, "Database design and SQL", "Databases", 2 }
            });

        migrationBuilder.InsertData(
            table: "Courses",
            columns: new[] { "Id", "CategoryId", "Description", "InstructorId", "IsPublished", "Level", "Price", "PublishedAt", "Title" },
            values: new object[,]
            {
                { 1, 3, "Learn the fundamentals of C# programming.", 1, true, 1, 49.99m, new DateTime(2025, 4, 1), "C# Fundamentals" },
                { 2, 3, "Build REST APIs using ASP.NET Core.", 1, true, 2, 89.99m, new DateTime(2025, 5, 1), "ASP.NET Core Web API" },
                { 3, 4, "Create responsive web page layouts.", 2, true, 1, 29.99m, new DateTime(2025, 5, 15), "HTML and CSS Foundations" },
                { 4, 5, "Analyze data using Python.", 3, true, 2, 99.99m, new DateTime(2025, 6, 1), "Python for Data Analysis" },
                { 5, 6, "An introduction to SQL Server and relational databases.", 2, false, 1, 59.99m, null, "SQL Server Essentials" },
                { 6, 3, "Advanced EF Core queries, tracking and performance.", 1, false, 3, 119.99m, null, "Advanced Entity Framework Core" }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(table: "Courses", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5, 6 });

        // Delete child categories before their parents because the self-reference is NO ACTION.
        migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValue: 3);
        migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValue: 4);
        migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValue: 6);
        migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValue: 2);
        migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValue: 1);
        migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValue: 5);

        migrationBuilder.DeleteData(table: "Instructors", keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
    }
}
