using Databasteknik.Application.Contracts;
using Databasteknik.Domain.Entities;
using Databasteknik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Databasteknik.Infrastructure.Services;

internal sealed class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _db;

    public EnrollmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EnrollResult> EnrollAsync(Guid participantId, Guid courseOccasionId, CancellationToken ct)
    {
        // Транзакция: все проверки + insert должны быть атомарны
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var participantExists = await _db.Participants.AnyAsync(p => p.Id == participantId, ct);
            if (!participantExists)
                return EnrollResult.Fail("ParticipantId not found.");

            var occasion = await _db.CourseOccasions
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == courseOccasionId, ct);

            if (occasion is null)
                return EnrollResult.Fail("CourseOccasionId not found.");

            // Проверка дубликата
            var already = await _db.Enrollments.AnyAsync(e =>
                e.ParticipantId == participantId &&
                e.CourseOccasionId == courseOccasionId, ct);

            if (already)
                return EnrollResult.Fail("Participant is already enrolled for this occasion.");

            // Проверка capacity (места)
            var usedSeats = await _db.Enrollments.CountAsync(e => e.CourseOccasionId == courseOccasionId, ct);

            if (occasion.Capacity > 0 && usedSeats >= occasion.Capacity)
                return EnrollResult.Fail("Course occasion is full.");

            var enrollment = new Enrollment
            {
                ParticipantId = participantId,
                CourseOccasionId = courseOccasionId,
                EnrolledAt = DateTime.UtcNow
            };

            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return EnrollResult.Ok(enrollment.Id);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}