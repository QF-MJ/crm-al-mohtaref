namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// The seed contract. One class per entity, placed in the folder of its feature:
//   Data\Seed\Users\...      Data\Seed\Contacts\...      Data\Seed\Campaigns\...
//
// To seed a new entity: add ONE new class (inherit EntitySeed<T>) in the feature folder.
// DataSeeder finds it by reflection. No existing file changes.
//
// Order = run order, lower first. Each Order must be unique (DataSeeder throws on duplicates).
// Convention: Users 100-199, shared lookups 200-299, campaigns 300-399, contacts 400-499.
// Pick a free number after the seeds your entity depends on.
public interface ISeed
{
    int Order { get; }

    Task SeedAsync(AppDbContext db, CancellationToken ct);
}