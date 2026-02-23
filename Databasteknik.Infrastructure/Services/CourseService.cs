using Databasteknik.Application.Contracts;
using Databasteknik.Domain.Entities;
using Databasteknik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Databasteknik.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db) => _db = db;

    public async Task<List<Course>> GetAllAsync(CancellationToken ct = default)
        => await _db.Courses.AsNoTracking().OrderBy(c => c.Title).ToListAsync(ct);

    public async Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Course> CreateAsync(Course course, CancellationToken ct = default)
    {
        _db.Courses.Add(course);
        await _db.SaveChangesAsync(ct);
        return course;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return false;
        _db.Courses.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
