using Databasteknik.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Databasteknik.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<CourseOccasion> CourseOccasions => Set<CourseOccasion>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Teacher>()
            .HasIndex(t => t.Email)
            .IsUnique();

        modelBuilder.Entity<Participant>()
            .HasIndex(p => p.Email)
            .IsUnique();

        modelBuilder.Entity<CourseOccasion>()
            .HasOne(o => o.Course)
            .WithMany(c => c.Occasions)
            .HasForeignKey(o => o.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseOccasion>()
            .HasOne(o => o.Teacher)
            .WithMany(t => t.Occasions)
            .HasForeignKey(o => o.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.CourseOccasionId, e.ParticipantId })
            .IsUnique();
    }
}
