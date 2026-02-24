using Databasteknik.Application.Contracts;
using Databasteknik.Domain.Entities;
using Databasteknik.Infrastructure;
using Databasteknik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");

// Apply migrations automatically (for demo)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => Results.Ok(new { status = "ok" }));

// -------------------- Courses CRUD (service-based) --------------------
app.MapGet("/api/courses", async (ICourseService svc, IMemoryCache cache, CancellationToken ct) =>
{
    const string key = "courses:all";

    var items = await cache.GetOrCreateAsync(key, async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
        entry.SlidingExpiration = TimeSpan.FromSeconds(10);

        var data = await svc.GetAllAsync(ct);
        return data;
    });

    return Results.Ok(items);
});

app.MapGet("/api/courses/{id:guid}", async (Guid id, ICourseService svc, CancellationToken ct) =>
{
    var item = await svc.GetByIdAsync(id, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/courses", async (Course input, ICourseService svc, IMemoryCache cache, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest("Title is required.");

    var created = await svc.CreateAsync(input, ct);

    cache.Remove("courses:all");

    return Results.Created($"/api/courses/{created.Id}", created);
});

app.MapDelete("/api/courses/{id:guid}", async (Guid id, ICourseService svc, IMemoryCache cache, CancellationToken ct) =>
{
    var ok = await svc.DeleteAsync(id, ct);

    if (ok) cache.Remove("courses:all");

    return ok ? Results.NoContent() : Results.NotFound();
});

// -------------------- Participants CRUD (db-based) --------------------
app.MapGet("/api/participants", async (AppDbContext db, IMemoryCache cache, CancellationToken ct) =>
{
    const string key = "participants:all";

    var items = await cache.GetOrCreateAsync(key, async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
        entry.SlidingExpiration = TimeSpan.FromSeconds(10);

        return await db.Participants.AsNoTracking().ToListAsync(ct);
    });

    return Results.Ok(items);
});

app.MapGet("/api/participants/{id:guid}", async (AppDbContext db, Guid id, CancellationToken ct) =>
{
    var item = await db.Participants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/participants", async (AppDbContext db, IMemoryCache cache, Participant participant, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(participant.Email))
        return Results.BadRequest("Email is required.");

    db.Participants.Add(participant);
    await db.SaveChangesAsync(ct);

    
    cache.Remove("participants:all");

    return Results.Created($"/api/participants/{participant.Id}", participant);
});

app.MapDelete("/api/participants/{id:guid}", async (AppDbContext db, IMemoryCache cache, Guid id, CancellationToken ct) =>
{
    var item = await db.Participants.FindAsync(new object[] { id }, ct);
    if (item is null) return Results.NotFound();

    db.Participants.Remove(item);
    await db.SaveChangesAsync(ct);

    cache.Remove("participants:all");

    return Results.NoContent();
});

// -------------------- Teachers CRUD (db-based) --------------------
app.MapGet("/api/teachers", async (AppDbContext db, CancellationToken ct) =>
{
    var items = await db.Teachers.AsNoTracking().ToListAsync(ct);
    return Results.Ok(items);
});

app.MapGet("/api/teachers/{id:guid}", async (AppDbContext db, Guid id, CancellationToken ct) =>
{
    var item = await db.Teachers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/teachers", async (AppDbContext db, Teacher teacher, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(teacher.Email))
        return Results.BadRequest("Email is required.");

    db.Teachers.Add(teacher);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/teachers/{teacher.Id}", teacher);
});

app.MapDelete("/api/teachers/{id:guid}", async (AppDbContext db, Guid id, CancellationToken ct) =>
{
    var item = await db.Teachers.FindAsync(new object[] { id }, ct);
    if (item is null) return Results.NotFound();

    db.Teachers.Remove(item);
    await db.SaveChangesAsync(ct);

    return Results.NoContent();
});

// -------------------- CourseOccasions CRUD --------------------
app.MapGet("/api/occasions", async (AppDbContext db, CancellationToken ct) =>
{
    var items = await db.CourseOccasions
        .AsNoTracking()
        .Include(x => x.Course)
        .Include(x => x.Teacher)
        .ToListAsync(ct);

    return Results.Ok(items);
});

app.MapGet("/api/occasions/{id:guid}", async (AppDbContext db, Guid id, CancellationToken ct) =>
{
    var item = await db.CourseOccasions
        .AsNoTracking()
        .Include(x => x.Course)
        .Include(x => x.Teacher)
        .FirstOrDefaultAsync(x => x.Id == id, ct);

    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/occasions", async (AppDbContext db, CourseOccasion input, CancellationToken ct) =>
{
    var courseExists = await db.Courses.AnyAsync(c => c.Id == input.CourseId, ct);
    if (!courseExists) return Results.BadRequest("CourseId not found.");

    var teacherExists = await db.Teachers.AnyAsync(t => t.Id == input.TeacherId, ct);
    if (!teacherExists) return Results.BadRequest("TeacherId not found.");

    if (input.EndDate < input.StartDate)
        return Results.BadRequest("EndDate must be >= StartDate.");

    db.CourseOccasions.Add(input);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/occasions/{input.Id}", input);
});

app.MapDelete("/api/occasions/{id:guid}", async (AppDbContext db, Guid id, CancellationToken ct) =>
{
    var item = await db.CourseOccasions.FindAsync(new object[] { id }, ct);
    if (item is null) return Results.NotFound();

    db.CourseOccasions.Remove(item);
    await db.SaveChangesAsync(ct);

    return Results.NoContent();
});


// -------------------- Enrollments (Registrations) --------------------
app.MapGet("/api/enrollments", async (AppDbContext db, CancellationToken ct) =>
{
    var items = await db.Enrollments
        .AsNoTracking()
        .Include(x => x.Participant)
        .Include(x => x.CourseOccasion)
            .ThenInclude(o => o.Course)
        .ToListAsync(ct);

    return Results.Ok(items);
});
app.MapPost("/api/enrollments", async (Databasteknik.Application.Contracts.IEnrollmentService svc, Enrollment input, CancellationToken ct) =>
{
    var result = await svc.EnrollAsync(input.ParticipantId, input.CourseOccasionId, ct);

    if (!result.Success)
    {
        if (result.Error?.Contains("already enrolled") == true) return Results.Conflict(result.Error);
        if (result.Error?.Contains("not found") == true) return Results.BadRequest(result.Error);
        if (result.Error?.Contains("full") == true) return Results.Conflict(result.Error);

        return Results.BadRequest(result.Error ?? "Enrollment failed.");
    }

    return Results.Created($"/api/enrollments/{result.EnrollmentId}", new { id = result.EnrollmentId });
});

app.MapDelete("/api/enrollments/{id:guid}", async (AppDbContext db, Guid id, CancellationToken ct) =>
{
    var item = await db.Enrollments.FindAsync(new object[] { id }, ct);
    if (item is null) return Results.NotFound();

    db.Enrollments.Remove(item);
    await db.SaveChangesAsync(ct);

    return Results.NoContent();
});

app.MapGet("/api/reports/course-enrollments", async (int? min, Databasteknik.Application.Contracts.IReportsService svc, CancellationToken ct) =>
{
    var result = await svc.GetCourseEnrollmentsAsync(min ?? 0, ct);
    return Results.Ok(result);
});

app.Run();