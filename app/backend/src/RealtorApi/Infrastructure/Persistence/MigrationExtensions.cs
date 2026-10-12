using Microsoft.EntityFrameworkCore;

namespace RealtorApi.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static async Task MigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (app.Environment.IsEnvironment("Testing") || !context.Database.IsRelational())
        {
            await context.Database.EnsureCreatedAsync();
            return;
        }

        await context.Database.MigrateAsync();
    }
}
