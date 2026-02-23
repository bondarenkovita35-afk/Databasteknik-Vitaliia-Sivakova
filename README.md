https://github.com/bondarenkovita35-afk/Databasteknik-Vitaliia-Sivakova

# Databasteknik (net9 + EF Core 9)

Backend for an education company: courses, course occasions, teachers, participants and enrollments.

## Tech
- .NET 9 (net9.0)
- EF Core 9.0.3
- SQLite (file database)
- Minimal API + Swagger
- xUnit test (EF InMemory)

## Run
From repo root:

```powershell
dotnet restore
dotnet build
dotnet run --project .\Databasteknik.Presentation
```

Swagger: `https://localhost:7001/swagger`

## Migrations
If you want to create migrations manually:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.0
dotnet ef migrations add InitialCreate --project .\Databasteknik.Infrastructure --startup-project .\Databasteknik.Presentation
dotnet ef database update --project .\Databasteknik.Infrastructure --startup-project .\Databasteknik.Presentation
```
