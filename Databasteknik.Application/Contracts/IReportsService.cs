namespace Databasteknik.Application.Contracts;

public interface IReportsService
{
    Task<IReadOnlyList<CourseEnrollmentReportDto>> GetCourseEnrollmentsAsync(int minEnrollments, CancellationToken ct);
}