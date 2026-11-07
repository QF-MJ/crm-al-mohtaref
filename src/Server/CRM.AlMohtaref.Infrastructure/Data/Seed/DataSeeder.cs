using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// Fake data for development. Runs only when the Users table is empty.
// The data itself lives in Data\Seed\<Entity>Seed.cs, this class only orders and saves it.
//
// Person.CreatedByUserId points to User and User.PersonId points to Person (circular FK),
// so the Person FK checks are switched off while inserting and validated again at the end.
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(ct);

        if (await db.Set<User>().AnyAsync(ct))
            return;

        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // 1) lookups
            var departments = DepartmentSeed.Create();
            var roles = RoleSeed.Create();
            db.Set<Department>().AddRange(departments);
            db.Set<Role>().AddRange(roles);
            await db.SaveChangesAsync(ct);

            // 2) persons (FK checks off because of the circular reference with User)
            var personEntity = db.Model.FindEntityType(typeof(Person))!;
            var personTable = "[" + (personEntity.GetSchema() ?? "dbo") + "].[" + personEntity.GetTableName() + "]";
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE " + personTable + " NOCHECK CONSTRAINT ALL", ct);

            var persons = PersonSeed.Create();
            db.Set<Person>().AddRange(persons);
            await db.SaveChangesAsync(ct);

            // 3) users
            var users = UserSeed.Create(persons);
            db.Set<User>().AddRange(users);
            await db.SaveChangesAsync(ct);

            // 4) permissions (department + role)
            db.Set<UserPermission>().AddRange(UserPermissionSeed.Create(users, departments, roles));
            await db.SaveChangesAsync(ct);

            // 5) everything was created by the admin user
            foreach (var person in persons)
                person.CreatedByUserId = users[0].Id;
            await db.SaveChangesAsync(ct);

            await db.Database.ExecuteSqlRawAsync("ALTER TABLE " + personTable + " WITH CHECK CHECK CONSTRAINT ALL", ct);

            await tx.CommitAsync(ct);
        });
    }
}
