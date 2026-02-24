namespace Databasteknik.Application.Contracts;

public sealed record CourseEnrollmentReportDto(
    Guid CourseId,
    string Title,
    int EnrollmentCount
);