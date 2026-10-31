using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OnlineLearningPlatform.Data;

#nullable disable

namespace OnlineLearningPlatform.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610310001_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Instructors",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                JoinedAt = table.Column<DateTime>(type: "date", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Instructors", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "date", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ParentCategoryId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Categories_Categories_ParentCategoryId",
                    column: x => x.ParentCategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                Level = table.Column<int>(type: "int", nullable: false),
                IsPublished = table.Column<bool>(type: "bit", nullable: false),
                PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                InstructorId = table.Column<int>(type: "int", nullable: false),
                CategoryId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Id);
                table.CheckConstraint("CK_Courses_Price_NonNegative", "[Price] >= 0");
                table.CheckConstraint(
                    "CK_Courses_PublishedDate",
                    "([IsPublished] = 0 AND [PublishedAt] IS NULL) OR ([IsPublished] = 1 AND [PublishedAt] IS NOT NULL)");
                table.ForeignKey(
                    name: "FK_Courses_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Courses_Instructors_InstructorId",
                    column: x => x.InstructorId,
                    principalTable: "Instructors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Enrollments",
            columns: table => new
            {
                StudentId = table.Column<int>(type: "int", nullable: false),
                CourseId = table.Column<int>(type: "int", nullable: false),
                EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ProgressPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                FinalGrade = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Enrollments", x => new { x.StudentId, x.CourseId });
                table.CheckConstraint(
                    "CK_Enrollments_ProgressPercentage",
                    "[ProgressPercentage] >= 0 AND [ProgressPercentage] <= 100");
                table.CheckConstraint(
                    "CK_Enrollments_FinalGrade_OnlyWhenCompleted",
                    "[Status] = 2 OR [FinalGrade] IS NULL");
                table.CheckConstraint(
                    "CK_Enrollments_StatusMatchesProgress",
                    "([Status] = 2 AND [ProgressPercentage] = 100) OR ([Status] <> 2 AND [ProgressPercentage] < 100)");
                table.ForeignKey(
                    name: "FK_Enrollments_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Enrollments_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Categories_ParentCategoryId",
            table: "Categories",
            column: "ParentCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Courses_CategoryId",
            table: "Courses",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Courses_InstructorId",
            table: "Courses",
            column: "InstructorId");

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_CourseId",
            table: "Enrollments",
            column: "CourseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Enrollments");
        migrationBuilder.DropTable(name: "Courses");
        migrationBuilder.DropTable(name: "Categories");
        migrationBuilder.DropTable(name: "Instructors");
        migrationBuilder.DropTable(name: "Students");
    }
}
