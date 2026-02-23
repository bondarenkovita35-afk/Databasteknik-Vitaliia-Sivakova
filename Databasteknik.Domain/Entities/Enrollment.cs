namespace Databasteknik.Domain.Entities;

public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CourseOccasionId { get; set; }
    public CourseOccasion CourseOccasion { get; set; } = null!;

    public Guid ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
