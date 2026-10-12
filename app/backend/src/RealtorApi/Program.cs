using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Environment.IsEnvironment("Testing")
    ? "Data Source=RealtorApiTest.db"
    : builder.Configuration.GetConnectionString("Default")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=realtor;Username=postgres;Password=postgres";

var useSqlite = builder.Environment.IsEnvironment("Testing")
    || connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase)
    || connectionString.StartsWith("Filename=", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }

    options.UseSeeding((context, _) =>
    {
        DatabaseSeeder.Seed((AppDbContext)context);
    })
    .UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        await DatabaseSeeder.SeedAsync((AppDbContext)context, cancellationToken);
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddSlices(typeof(Program).Assembly);
builder.Services.AddHandlers(typeof(Program).Assembly);

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "support", "seed-data", "images")),
    RequestPath = "/images"
});

app.MapSlices();

await app.MigrateAsync();

app.Run();

public partial class Program { }
