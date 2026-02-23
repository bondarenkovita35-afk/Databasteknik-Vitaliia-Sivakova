using Databasteknik.Application.Contracts;
using Databasteknik.Domain.Entities;
using Databasteknik.Infrastructure;
using Databasteknik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply migrations automatically (for demo/dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => Results.Ok(new { status = "ok" }));

// Courses CRUD (basic)
app.MapGet("/api/courses", async (ICourseService svc, CancellationToken ct) =>
{
    var items = await svc.GetAllAsync(ct);
    return Results.Ok(items);
});

app.MapGet("/api/courses/{id:guid}", async (Guid id, ICourseService svc, CancellationToken ct) =>
{
    var item = await svc.GetByIdAsync(id, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/courses", async (Course input, ICourseService svc, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest("Title is required.");

    var created = await svc.CreateAsync(input, ct);
    return Results.Created($"/api/courses/{created.Id}", created);
});

app.MapDelete("/api/courses/{id:guid}", async (Guid id, ICourseService svc, CancellationToken ct) =>
{
    var ok = await svc.DeleteAsync(id, ct);
    return ok ? Results.NoContent() : Results.NotFound();
});

app.Run();
