using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Entities;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("========== ONLINE LEARNING PLATFORM ==========");

        // Applies both migrations: schema first, then HasData reference data.
        await using (var db = new AppDbContext())
        {
            await db.Database.MigrateAsync();
        }

        await SeedStudentsAndEnrollmentsAsync();

        Console.WriteLine("\n================ CREATE ================");
        int instructorId = await AddInstructorAsync();
        int newCourseId = await AddCourseAsync(instructorId, categoryId: 3);
        List<int> newStudentIds = await AddSeveralStudentsAsync();

        if (newStudentIds.Count > 0)
        {
            await EnrollStudentAsync(newStudentIds[0], courseId: 1);
        }

        Console.WriteLine("\n================ READ ================");
        await FindCourseAsync(courseId: 1, title: "C# Fundamentals");
        await FilterCoursesByPriceAsync(minPrice: 20m, maxPrice: 100m);
        await PrintSubCategoriesAsync(parentCategoryId: 2);
        await PrintTopLevelCategoriesAsync();
        await PrintCourseRosterAsync(courseId: 1);

        Console.WriteLine("\n================ UPDATE ================");
        await ChangeCoursePriceAsync(courseId: 6, newPrice: 109.99m);

        if (newStudentIds.Count > 0)
        {
            await UpdateEnrollmentProgressAsync(newStudentIds[0], courseId: 1, newProgress: 100m);
        }

        await PublishCourseAsync(courseId: 6);

        Console.WriteLine("\n================ DELETE ================");
        if (newStudentIds.Count > 0)
        {
            await UnenrollStudentAsync(newStudentIds[0], courseId: 1);
        }

        await TryDeleteCategoryWithSubCategoriesAsync(categoryId: 1);
        await DeleteCategorySafelyAsync(categoryId: 1);
        await DeleteCourseAndReportEnrollmentsAsync(courseId: 1);

        Console.WriteLine("\n================ BULK OPERATIONS ================");
        await RaisePublishedCoursePricesAsync(CourseLevel.Intermediate);
        await DeleteDroppedEnrollmentsAsync();

        Console.WriteLine("\n========== DEMO FINISHED ==========");
        Console.WriteLine($"New instructor Id: {instructorId}");
        Console.WriteLine($"New course Id: {newCourseId}");
        Console.WriteLine($"New student Ids: {string.Join(", ", newStudentIds)}");
    }

    // -------------------- PART 02: RUNTIME SEEDING --------------------

    private static async Task SeedStudentsAndEnrollmentsAsync()
    {
        await using var db = new AppDbContext();

        // Add the five students only when Students is empty.
        if (!await db.Students.AnyAsync())
        {
            var students = new List<Student>
            {
                new() { FullName = "Ali Mohamed", Email = "ali.mohamed@student.com", DateOfBirth = new DateTime(2001, 3, 12) },
                new() { FullName = "Sara Ahmed", Email = "sara.ahmed@student.com", DateOfBirth = new DateTime(2002, 7, 21) },
                new() { FullName = "Youssef Adel", Email = "youssef.adel@student.com", DateOfBirth = new DateTime(2000, 11, 5) },
                new() { FullName = "Nour Hassan", Email = "nour.hassan@student.com", DateOfBirth = new DateTime(2003, 1, 17) },
                new() { FullName = "Mina Samir", Email = "mina.samir@student.com", DateOfBirth = new DateTime(2001, 9, 30) }
            };

            db.Students.AddRange(students);
            await db.SaveChangesAsync();
            Console.WriteLine($"Runtime seeding: inserted {students.Count} students.");
        }
        else
        {
            Console.WriteLine("Runtime seeding: Students table is not empty; skipped.");
        }

        // Enrollments are also seeded only when their own table is empty.
        if (!await db.Enrollments.AnyAsync())
        {
            List<Student> students = await db.Students
                .OrderBy(s => s.Id)
                .Take(5)
                .ToListAsync();

            bool requiredCoursesExist = await db.Courses
                .Where(c => c.IsPublished && new[] { 1, 2, 3, 4 }.Contains(c.Id))
                .CountAsync() == 4;

            if (students.Count < 5 || !requiredCoursesExist)
            {
                Console.WriteLine("Runtime enrollment seeding skipped: five students and four published seed courses are required.");
                return;
            }

            DateTime now = DateTime.Now;
            var enrollments = new List<Enrollment>
            {
                new() { StudentId = students[0].Id, CourseId = 1, EnrolledAt = now.AddDays(-20), ProgressPercentage = 35m, Status = EnrollmentStatus.Active },
                new() { StudentId = students[0].Id, CourseId = 2, EnrolledAt = now.AddDays(-30), ProgressPercentage = 100m, Status = EnrollmentStatus.Completed, FinalGrade = 92m },
                new() { StudentId = students[1].Id, CourseId = 1, EnrolledAt = now.AddDays(-15), ProgressPercentage = 70m, Status = EnrollmentStatus.Active },
                new() { StudentId = students[1].Id, CourseId = 3, EnrolledAt = now.AddDays(-25), ProgressPercentage = 100m, Status = EnrollmentStatus.Completed, FinalGrade = 88m },
                new() { StudentId = students[2].Id, CourseId = 2, EnrolledAt = now.AddDays(-12), ProgressPercentage = 20m, Status = EnrollmentStatus.Dropped },
                new() { StudentId = students[3].Id, CourseId = 4, EnrolledAt = now.AddDays(-10), ProgressPercentage = 15m, Status = EnrollmentStatus.Active },
                new() { StudentId = students[3].Id, CourseId = 1, EnrolledAt = now.AddDays(-35), ProgressPercentage = 100m, Status = EnrollmentStatus.Completed, FinalGrade = 76m },
                new() { StudentId = students[4].Id, CourseId = 3, EnrolledAt = now.AddDays(-8), ProgressPercentage = 40m, Status = EnrollmentStatus.Dropped }
            };

            db.Enrollments.AddRange(enrollments);
            await db.SaveChangesAsync();
            Console.WriteLine($"Runtime seeding: inserted {enrollments.Count} enrollments.");
        }
        else
        {
            Console.WriteLine("Runtime seeding: Enrollments table is not empty; skipped.");
        }
    }

    // -------------------- CREATE --------------------

    private static async Task<int> AddInstructorAsync()
    {
        await using var db = new AppDbContext();

        var instructor = new Instructor
        {
            FullName = "Mona Ibrahim",
            Email = "mona.ibrahim@learnhub.com",
            Bio = "Software engineering instructor.",
            JoinedAt = DateTime.Today
        };

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync();

        Console.WriteLine($"Instructor added successfully. Generated Id = {instructor.Id}");
        return instructor.Id;
    }

    private static async Task<int> AddCourseAsync(int instructorId, int categoryId)
    {
        await using var db = new AppDbContext();

        if (!await db.Instructors.AnyAsync(i => i.Id == instructorId))
        {
            Console.WriteLine($"Cannot add course: instructor Id {instructorId} does not exist.");
            return 0;
        }

        if (!await db.Categories.AnyAsync(c => c.Id == categoryId))
        {
            Console.WriteLine($"Cannot add course: category Id {categoryId} does not exist.");
            return 0;
        }

        var course = new Course
        {
            Title = "Clean Code with C#",
            Description = "Practical clean-code principles for C# developers.",
            Price = 69.99m,
            Level = CourseLevel.Advanced,
            IsPublished = false,
            PublishedAt = null,
            InstructorId = instructorId,
            CategoryId = categoryId
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        Console.WriteLine($"Course added successfully. Generated Id = {course.Id}");
        return course.Id;
    }

    private static async Task<List<int>> AddSeveralStudentsAsync()
    {
        await using var db = new AppDbContext();

        var students = new List<Student>
        {
            new() { FullName = "Hana Mahmoud", Email = "hana.mahmoud@students.com", DateOfBirth = new DateTime(2002, 2, 14) },
            new() { FullName = "Karim Tarek", Email = "karim.tarek@students.com", DateOfBirth = new DateTime(2001, 6, 8) },
            new() { FullName = "Salma Wael", Email = "salma.wael@students.com", DateOfBirth = new DateTime(2003, 10, 19) }
        };

        // One AddRange call and exactly one SaveChanges call for the three students.
        db.Students.AddRange(students);
        await db.SaveChangesAsync();

        List<int> generatedIds = students.Select(s => s.Id).ToList();
        Console.WriteLine($"Inserted three students. Generated Ids: {string.Join(", ", generatedIds)}");
        return generatedIds;
    }

    private static async Task EnrollStudentAsync(int studentId, int courseId)
    {
        await using var db = new AppDbContext();

        if (!await db.Students.AnyAsync(s => s.Id == studentId))
        {
            Console.WriteLine($"Enrollment refused: student Id {studentId} does not exist.");
            return;
        }

        Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null)
        {
            Console.WriteLine($"Enrollment refused: course Id {courseId} does not exist.");
            return;
        }

        if (!course.IsPublished)
        {
            Console.WriteLine($"Enrollment refused: course '{course.Title}' is not published.");
            return;
        }

        bool alreadyEnrolled = await db.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        if (alreadyEnrolled)
        {
            Console.WriteLine("Enrollment refused: this student is already enrolled in this course.");
            return;
        }

        db.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.Now,
            ProgressPercentage = 0m,
            Status = EnrollmentStatus.Active,
            FinalGrade = null
        });

        await db.SaveChangesAsync();
        Console.WriteLine($"Student {studentId} enrolled in course {courseId} successfully.");
    }

    // -------------------- READ --------------------

    private static async Task FindCourseAsync(int courseId, string title)
    {
        await using var db = new AppDbContext();

        Course? byId = await db.Courses.FindAsync(courseId);
        Console.WriteLine(byId is null
            ? $"Course with Id {courseId} not found."
            : $"Find by Id: {byId.Title} | Price: {byId.Price:C}");

        Course? byTitle = await db.Courses.FirstOrDefaultAsync(c => c.Title == title);
        Console.WriteLine(byTitle is null
            ? $"Course with title '{title}' not found."
            : $"FirstOrDefault by title: Id = {byTitle.Id}, Level = {byTitle.Level}");
    }

    private static async Task FilterCoursesByPriceAsync(decimal minPrice, decimal maxPrice)
    {
        await using var db = new AppDbContext();

        if (minPrice < 0 || maxPrice < minPrice)
        {
            Console.WriteLine("Invalid price range. Make sure prices are non-negative and maximum is not less than minimum.");
            return;
        }

        var courses = await db.Courses
            .Where(c => c.Price >= minPrice && c.Price <= maxPrice)
            .OrderByDescending(c => c.Price)
            .Select(c => new { c.Title, c.Level, c.Price })
            .ToListAsync();

        Console.WriteLine($"Courses priced between {minPrice:C} and {maxPrice:C} (highest price first):");
        if (courses.Count == 0)
        {
            Console.WriteLine("No courses match this price range.");
            return;
        }

        foreach (var course in courses)
        {
            Console.WriteLine($"- {course.Title} | {course.Level} | {course.Price:C}");
        }
    }

    private static async Task PrintSubCategoriesAsync(int parentCategoryId)
    {
        await using var db = new AppDbContext();

        if (!await db.Categories.AnyAsync(c => c.Id == parentCategoryId))
        {
            Console.WriteLine($"Category Id {parentCategoryId} not found.");
            return;
        }

        var children = await db.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        Console.WriteLine($"Direct sub-categories of category {parentCategoryId}:");
        if (children.Count == 0)
        {
            Console.WriteLine("No direct sub-categories.");
            return;
        }

        foreach (var child in children)
        {
            Console.WriteLine($"- {child.Id}: {child.Name}");
        }
    }

    private static async Task PrintTopLevelCategoriesAsync()
    {
        await using var db = new AppDbContext();

        var categories = await db.Categories
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        Console.WriteLine("Top-level categories:");
        foreach (var category in categories)
        {
            Console.WriteLine($"- {category.Id}: {category.Name}");
        }
    }

    private static async Task PrintCourseRosterAsync(int courseId)
    {
        await using var db = new AppDbContext();

        if (!await db.Courses.AnyAsync(c => c.Id == courseId))
        {
            Console.WriteLine($"Cannot print roster: course Id {courseId} was not found.");
            return;
        }

        // Query 1: get the student IDs from Enrollments (no join and no Include).
        List<int> studentIds = await db.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.StudentId)
            .ToListAsync();

        Console.WriteLine($"Course {courseId} has {studentIds.Count} enrolled student(s).");
        if (studentIds.Count == 0)
        {
            return;
        }

        // Query 2: get students using Contains on the IDs returned by query 1.
        var students = await db.Students
            .Where(s => studentIds.Contains(s.Id))
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName })
            .ToListAsync();

        foreach (var student in students)
        {
            Console.WriteLine($"- {student.Id}: {student.FullName}");
        }
    }

    // -------------------- UPDATE --------------------

    private static async Task ChangeCoursePriceAsync(int courseId, decimal newPrice)
    {
        await using var db = new AppDbContext();

        if (newPrice < 0)
        {
            Console.WriteLine("Invalid price: price cannot be negative.");
            return;
        }

        Course? course = await db.Courses.FindAsync(courseId);
        if (course is null)
        {
            Console.WriteLine($"Cannot change price: course Id {courseId} was not found.");
            return;
        }

        decimal oldPrice = course.Price;
        course.Price = newPrice;
        await db.SaveChangesAsync();

        Console.WriteLine($"Course '{course.Title}' price changed from {oldPrice:C} to {newPrice:C}.");
    }

    private static async Task UpdateEnrollmentProgressAsync(int studentId, int courseId, decimal newProgress)
    {
        await using var db = new AppDbContext();

        if (newProgress < 0m || newProgress > 100m)
        {
            Console.WriteLine("Invalid progress: enter a value from 0 to 100.");
            return;
        }

        // FindAsync receives both values of the composite primary key.
        Enrollment? enrollment = await db.Enrollments.FindAsync(studentId, courseId);
        if (enrollment is null)
        {
            Console.WriteLine($"Enrollment for student {studentId} and course {courseId} was not found.");
            return;
        }

        enrollment.ProgressPercentage = newProgress;
        if (newProgress == 100m)
        {
            enrollment.Status = EnrollmentStatus.Completed;
        }
        else if (enrollment.Status == EnrollmentStatus.Completed)
        {
            enrollment.Status = EnrollmentStatus.Active;
            enrollment.FinalGrade = null;
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"Progress updated to {newProgress}% (Status: {enrollment.Status}).");
    }

    private static async Task PublishCourseAsync(int courseId)
    {
        await using var db = new AppDbContext();

        Course? course = await db.Courses.FindAsync(courseId);
        if (course is null)
        {
            Console.WriteLine($"Cannot publish: course Id {courseId} was not found.");
            return;
        }

        if (course.IsPublished)
        {
            Console.WriteLine($"Course '{course.Title}' is already published.");
            return;
        }

        course.IsPublished = true;
        course.PublishedAt = DateTime.Now;
        await db.SaveChangesAsync();

        Console.WriteLine($"Course '{course.Title}' published at {course.PublishedAt:G}.");
    }

    // -------------------- DELETE --------------------

    private static async Task UnenrollStudentAsync(int studentId, int courseId)
    {
        await using var db = new AppDbContext();

        Enrollment? enrollment = await db.Enrollments.FindAsync(studentId, courseId);
        if (enrollment is null)
        {
            Console.WriteLine($"Cannot unenroll: enrollment ({studentId}, {courseId}) was not found.");
            return;
        }

        db.Enrollments.Remove(enrollment);
        await db.SaveChangesAsync();
        Console.WriteLine($"Student {studentId} was unenrolled from course {courseId}.");
    }

    private static async Task TryDeleteCategoryWithSubCategoriesAsync(int categoryId)
    {
        await using var db = new AppDbContext();

        Category? category = await db.Categories.FindAsync(categoryId);
        if (category is null)
        {
            Console.WriteLine($"Category Id {categoryId} was not found; nothing to delete.");
            return;
        }

        db.Categories.Remove(category);
        try
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"Category '{category.Name}' deleted.");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("Delete blocked by the database because related rows still reference this category.");
            Console.WriteLine($"Database message: {ex.GetBaseException().Message}");
        }
    }

    private static async Task DeleteCategorySafelyAsync(int categoryId)
    {
        await using var db = new AppDbContext();

        Category? category = await db.Categories.FindAsync(categoryId);
        if (category is null)
        {
            Console.WriteLine($"Safe delete: category Id {categoryId} was not found.");
            return;
        }

        bool hasSubCategories = await db.Categories.AnyAsync(c => c.ParentCategoryId == categoryId);
        if (hasSubCategories)
        {
            Console.WriteLine($"Safe delete refused: '{category.Name}' still has sub-categories.");
            return;
        }

        bool hasCourses = await db.Courses.AnyAsync(c => c.CategoryId == categoryId);
        if (hasCourses)
        {
            Console.WriteLine($"Safe delete refused: '{category.Name}' still contains courses.");
            return;
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        Console.WriteLine($"Category '{category.Name}' deleted safely.");
    }

    private static async Task DeleteCourseAndReportEnrollmentsAsync(int courseId)
    {
        await using var db = new AppDbContext();

        Course? course = await db.Courses.FindAsync(courseId);
        if (course is null)
        {
            Console.WriteLine($"Cannot delete course: Id {courseId} was not found.");
            return;
        }

        int before = await db.Enrollments.CountAsync(e => e.CourseId == courseId);
        db.Courses.Remove(course);
        await db.SaveChangesAsync();

        // The Course -> Enrollment FK is configured with ON DELETE CASCADE.
        // SQL Server therefore removes the course's dependent enrollment rows automatically.
        int after = await db.Enrollments.CountAsync(e => e.CourseId == courseId);

        Console.WriteLine($"Course '{course.Title}' deleted.");
        Console.WriteLine($"Enrollments before: {before}; after: {after}; removed by cascade: {before - after}.");
    }

    // -------------------- BULK OPERATIONS --------------------

    private static async Task RaisePublishedCoursePricesAsync(CourseLevel level)
    {
        await using var db = new AppDbContext();

        int affectedRows = await db.Courses
            .Where(c => c.IsPublished && c.Level == level)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Price, c => c.Price * 1.10m));

        Console.WriteLine($"Bulk price raise: increased published {level} course prices by 10%. Rows affected = {affectedRows}.");
    }

    private static async Task DeleteDroppedEnrollmentsAsync()
    {
        await using var db = new AppDbContext();

        int affectedRows = await db.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Dropped)
            .ExecuteDeleteAsync();

        Console.WriteLine($"Bulk delete: removed Dropped enrollments. Rows affected = {affectedRows}.");
    }
}
