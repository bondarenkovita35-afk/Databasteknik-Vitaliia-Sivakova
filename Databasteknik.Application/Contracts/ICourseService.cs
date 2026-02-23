using Databasteknik.Domain.Entities;

namespace Databasteknik.Application.Contracts;

public interface ICourseService
{
    Task<List<Course>> GetAllAsync(CancellationToken ct = default);
    Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Course> CreateAsync(Course course, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
