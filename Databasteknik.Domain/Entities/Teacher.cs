namespace Databasteknik.Domain.Entities;

public class Teacher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<CourseOccasion> Occasions { get; set; } = new();
}
