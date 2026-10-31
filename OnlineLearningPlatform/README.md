# Online Learning Platform — Session 03 Assignment

A .NET 10 Console Application using EF Core 10 (Code First) and SQL Server.

## Requirements

- .NET 10 SDK
- SQL Server, SQL Server Express, or SQL Server LocalDB
- EF Core CLI (for migration commands)

## Packages

The project references:

- `Microsoft.EntityFrameworkCore.SqlServer` 10.0.14
- `Microsoft.EntityFrameworkCore.Design` 10.0.14

Install the matching EF CLI tool once:

```bash
dotnet tool install --global dotnet-ef --version 10.0.14
```

## Database connection

The default connection string is configured in `Data/AppDbContext.cs` for Windows LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=OnlineLearningPlatformDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

To use another SQL Server instance, set the `ONLINE_LEARNING_CONNECTION` environment variable.

PowerShell example:

```powershell
$env:ONLINE_LEARNING_CONNECTION = "Server=.;Database=OnlineLearningPlatformDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

## Run the application

From the folder containing `OnlineLearningPlatform.csproj`:

```bash
dotnet restore
dotnet run
```

`Program.cs` calls `Database.MigrateAsync()`. Therefore, the database and tables are created automatically and both migrations are applied before the runtime seed runs.

You can also apply the migrations explicitly:

```bash
dotnet ef database update
```

## Migrations

The migration files are deliberately separated:

1. `202610310001_InitialCreate` — creates all five tables, keys, indexes, foreign keys and check constraints. It does not insert the reference rows.
2. `202610310002_SeedReferenceData` — inserts the three instructors, six hierarchical categories and six courses defined with `HasData` in their configuration classes.

`AppDbContextModelSnapshot.cs` represents the final model, including the `HasData` definitions.

## Runtime seeding

After migrations are applied, `SeedStudentsAndEnrollmentsAsync()` inserts:

- Five students only when `Students` is empty.
- Eight enrollments only when `Enrollments` is empty.

The eight rows cover `Active`, `Completed`, and `Dropped`. Completed rows have 100% progress. The seed checks that the required published sample courses exist before creating enrollments.

The Create section later inserts three more students as a separate CRUD demonstration. Those are intentional operation examples, not part of the idempotent seed routine.

## Main implementation points

- Each entity has a separate `IEntityTypeConfiguration<TEntity>` class.
- All relationships use explicit foreign-key properties.
- `Category` uses a self-reference with `DeleteBehavior.Restrict`.
- `Enrollment` uses `(StudentId, CourseId)` as a composite primary key, preventing duplicate enrollment in the same course.
- Course deletion cascades to its enrollments; category deletion is restricted while dependent categories or courses exist.
- `ProgressPercentage` is constrained to 0–100. A non-completed enrollment cannot have a final grade.
- `ExecuteUpdateAsync` raises published course prices by 10% without loading entities.
- `ExecuteDeleteAsync` deletes all Dropped enrollments without loading entities.
- No `Include` or lazy loading is used. The roster is implemented with two direct DbSet queries and `Contains`.

## Useful SQL checks

Run these queries in SQL Server Management Studio after the application starts:

```sql
SELECT * FROM Instructors;
SELECT * FROM Categories ORDER BY ParentCategoryId, Id;
SELECT * FROM Courses ORDER BY Id;
SELECT * FROM Students ORDER BY Id;
SELECT * FROM Enrollments ORDER BY StudentId, CourseId;

SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;
```

The app demonstrates destructive operations too: it deletes the sample course with Id 1 and removes all Dropped enrollments at the end. Re-running the app will not reinsert the `HasData` or runtime seed records because those seeds are guarded by migrations / empty-table checks; CRUD demo inserts still run again by design.
