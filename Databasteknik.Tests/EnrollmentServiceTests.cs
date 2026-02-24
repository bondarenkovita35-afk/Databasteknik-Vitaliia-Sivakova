using Databasteknik.Application.Contracts;
using Databasteknik.Domain.Entities;
using Databasteknik.Infrastructure.Persistence;
using Databasteknik.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Databasteknik.Tests;

public class EnrollmentServiceTests
{
    private static async Task<AppDbContext> CreateSqliteInMemoryDbAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new AppDbContext(options);

    
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static async Task<(Guid courseId, Guid teacherId, Guid occasionId)> SeedCourseOccasionAsync(AppDbContext db)
    {
        var course = new Course
        {
            Title = "SQL 101",
            Description = "Intro",
            Credits = 5
        };

        var teacher = new Teacher
        {
            FirstName = "Anna",
            LastName = "Teacher",
            Email = $"teacher_{Guid.NewGuid():N}@example.com"
        };

        db.Courses.Add(course);
        db.Teachers.Add(teacher);
        await db.SaveChangesAsync();

        var occasion = new CourseOccasion
        {
            CourseId = course.Id,
            TeacherId = teacher.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            Location = "Stockholm",
            Capacity = 1
        };

        db.CourseOccasions.Add(occasion);
        await db.SaveChangesAsync();

        return (course.Id, teacher.Id, occasion.Id);
    }

    private static async Task<Guid> SeedParticipantAsync(AppDbContext db, string emailPrefix)
    {
        var p = new Participant
        {
            FirstName = "V",
            LastName = "P",
            Email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com"
        };

        db.Participants.Add(p);
        await db.SaveChangesAsync();
        return p.Id;
    }

    [Fact]
    public async Task Enroll_fails_when_duplicate_enrollment()
    {
       
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var db = await CreateSqliteInMemoryDbAsync(connection);
        var (_, _, occasionId) = await SeedCourseOccasionAsync(db);
        var participantId = await SeedParticipantAsync(db, "dup");

        IEnrollmentService svc = new EnrollmentService(db);

      
        var r1 = await svc.EnrollAsync(participantId, occasionId, CancellationToken.None);
        Assert.True(r1.Success);
        Assert.NotNull(r1.EnrollmentId);

      
        var r2 = await svc.EnrollAsync(participantId, occasionId, CancellationToken.None);
        Assert.False(r2.Success);
        Assert.NotNull(r2.Error);
        Assert.Contains("already enrolled", r2.Error!, StringComparison.OrdinalIgnoreCase);
        var count = await db.Enrollments.CountAsync(e => e.ParticipantId == participantId && e.CourseOccasionId == occasionId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Enroll_fails_when_capacity_is_full()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var db = await CreateSqliteInMemoryDbAsync(connection);
        var (_, _, occasionId) = await SeedCourseOccasionAsync(db);

        var p1 = await SeedParticipantAsync(db, "cap1");
        var p2 = await SeedParticipantAsync(db, "cap2");

        IEnrollmentService svc = new EnrollmentService(db);

       
        var r1 = await svc.EnrollAsync(p1, occasionId, CancellationToken.None);
        Assert.True(r1.Success);

       
        var r2 = await svc.EnrollAsync(p2, occasionId, CancellationToken.None);
        Assert.False(r2.Success);
        Assert.NotNull(r2.Error);
        Assert.Contains("full", r2.Error!, StringComparison.OrdinalIgnoreCase);

        // bara 1 enrollment
        var total = await db.Enrollments.CountAsync(e => e.CourseOccasionId == occasionId);
        Assert.Equal(1, total);
    }
}