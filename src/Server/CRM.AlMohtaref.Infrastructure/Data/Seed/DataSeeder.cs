using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// Fake data for development. Nothing to change when an entity or a property changes:
// AutoSeeder reads the EF model on every run. Hand-tuned values live in SeedRules.cs.
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(ct);

        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            await new AutoSeeder(db, ct).RunAsync();

            await tx.CommitAsync(ct);
        });
    }
}