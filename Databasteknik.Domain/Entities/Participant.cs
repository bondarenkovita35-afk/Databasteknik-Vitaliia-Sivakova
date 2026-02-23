namespace Databasteknik.Domain.Entities;

public class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<Enrollment> Enrollments { get; set; } = new();
}
