using Databasteknik.Application.Contracts;
using Databasteknik.Infrastructure.Persistence;
using Databasteknik.Infrastructure.Services;
using Databasteknik.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Databasteknik.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IReportsService, ReportsService>();
        var cs = config.GetConnectionString("DefaultConnection") ?? "Data Source=databasteknik.db";
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        services.AddScoped<ICourseService, CourseService>();

        return services;
    }
}
