namespace Databasteknik.Application.Contracts;

public interface IEnrollmentService
{
    Task<EnrollResult> EnrollAsync(Guid participantId, Guid courseOccasionId, CancellationToken ct);
}

public sealed record EnrollResult(bool Success, string? Error, Guid? EnrollmentId)
{
    public static EnrollResult Ok(Guid id) => new(true, null, id);
    public static EnrollResult Fail(string error) => new(false, error, null);
}