using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// Fake data for development. This class never changes when a new entity is added:
// it finds every ISeed in this assembly, orders them by Order and runs them all in ONE transaction
// (needed because Person <-> User reference each other, see PersonSeed / UserSeed).
// Every seed skips itself when its table already has data, so it is safe to run on every start.
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(ct);

        var seeds = Discover();
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            foreach (var seed in seeds)
            {
                await seed.SeedAsync(db, ct);
                db.ChangeTracker.Clear();
            }

            await tx.CommitAsync(ct);
        });
    }

    private static List<ISeed> Discover()
    {
        var seeds = typeof(DataSeeder).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ISeed).IsAssignableFrom(t))
            .Select(t => (ISeed)Activator.CreateInstance(t)!)
            .OrderBy(s => s.Order)
            .ToList();

        var duplicate = seeds.GroupBy(s => s.Order).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            var names = string.Join(", ", duplicate.Select(s => s.GetType().Name));
            throw new InvalidOperationException("Seeds share the same Order " + duplicate.Key + ": " + names);
        }

        return seeds;
    }
}