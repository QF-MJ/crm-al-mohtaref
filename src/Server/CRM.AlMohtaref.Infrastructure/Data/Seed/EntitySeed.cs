using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// Base class for the usual case: "if the table is empty, insert these rows".
// A derived class only says its Order and how to build its rows.
// Override SeedAsync only when the entity needs special handling (see PersonSeed / UserSeed).
internal abstract class EntitySeed<T> : ISeed where T : class
{
    public abstract int Order { get; }

    public virtual async Task SeedAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Set<T>().AnyAsync(ct))
            return;

        var items = await CreateAsync(db, ct);

        db.Set<T>().AddRange(items);
        await db.SaveChangesAsync(ct);
    }

    protected abstract Task<IReadOnlyList<T>> CreateAsync(AppDbContext db, CancellationToken ct);

    protected static Task<IReadOnlyList<T>> Result(IEnumerable<T> items)
    {
        return Task.FromResult<IReadOnlyList<T>>(items.ToList());
    }
}