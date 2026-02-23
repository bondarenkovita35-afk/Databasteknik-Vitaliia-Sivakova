namespace Databasteknik.Domain.Entities;

public class CourseOccasion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;

    public int Capacity { get; set; } = 20;

    public List<Enrollment> Enrollments { get; set; } = new();
}
