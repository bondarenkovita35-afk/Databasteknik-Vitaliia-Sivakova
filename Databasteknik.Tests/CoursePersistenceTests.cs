using Databasteknik.Domain.Entities;
using Databasteknik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Databasteknik.Tests;

public class CoursePersistenceTests
{
    [Fact]
    public async Task Can_create_and_read_course()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var db = new AppDbContext(options);

        var course = new Course { Title = "DevOps Basics", Description = "Intro", Credits = 5 };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var loaded = await db.Courses.FirstOrDefaultAsync(c => c.Id == course.Id);

        Assert.NotNull(loaded);
        Assert.Equal("DevOps Basics", loaded!.Title);
    }
}
