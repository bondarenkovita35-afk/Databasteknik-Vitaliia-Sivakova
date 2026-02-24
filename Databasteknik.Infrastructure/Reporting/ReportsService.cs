using Databasteknik.Application.Contracts;
using Databasteknik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Databasteknik.Infrastructure.Reporting;

internal sealed class ReportsService : IReportsService
{
    private readonly AppDbContext _db;

    public ReportsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CourseEnrollmentReportDto>> GetCourseEnrollmentsAsync(int minEnrollments, CancellationToken ct)
    {
        if (minEnrollments < 0) minEnrollments = 0;

        // Raw SQL via EF Core (SQLite compatible)
        var rows = await _db.CourseEnrollmentReportRows
            .FromSqlInterpolated($@"
             SELECT
             c.Id   AS CourseId,
             c.Title AS Title,
             COUNT(e.Id) AS EnrollmentCount
             FROM Courses c
             LEFT JOIN Enrollments e ON e.CourseOccasionId = o.Id
             GROUP BY c.Id, c.Title
             HAVING COUNT(e.Id) >= {minEnrollments}
             ORDER BY EnrollmentCount DESC, Title ASC;
             ")
            .AsNoTracking()
            .ToListAsync(ct);

        return rows
            .Select(r => new CourseEnrollmentReportDto(r.CourseId, r.Title, r.EnrollmentCount))
            .ToList();
    }
}
