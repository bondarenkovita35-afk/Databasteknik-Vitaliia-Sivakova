namespace Databasteknik.Infrastructure.Reporting;

internal sealed class CourseEnrollmentReportRow
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
}