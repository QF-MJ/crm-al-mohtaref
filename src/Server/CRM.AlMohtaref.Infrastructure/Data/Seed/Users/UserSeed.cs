using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Users;

internal sealed class UserSeed : EntitySeed<User>
{
    public const string AdminAccountName = "admin";

    // placeholder: replace with a real hash when IPasswordHasher / login is implemented
    public const string FakePasswordHash = "12345678";

    public override int Order => 130;

    public override async Task SeedAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Set<User>().AnyAsync(ct))
            return;

        await base.SeedAsync(db, ct);

        // close the circle opened by PersonSeed: everything was created by the admin
        var adminId = await SeedHelpers.GetAdminUserIdAsync(db, ct);
        var persons = await db.Set<Person>().Where(p => p.CreatedByUserId == 0).ToListAsync(ct);

        foreach (var person in persons)
            person.CreatedByUserId = adminId;

        await db.SaveChangesAsync(ct);

        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE " + SeedHelpers.TableName<Person>(db) + " WITH CHECK CHECK CONSTRAINT ALL", ct);
    }

    // persons[0] -> admin, persons[i] -> user01..user24 (same order as PersonSeed)
    protected override async Task<IReadOnlyList<User>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var persons = await db.Set<Person>()
            .Where(p => !db.Set<User>().Any(u => u.PersonId == p.Id))
            .OrderBy(p => p.Id)
            .ToListAsync(ct);

        PasswordHasher password = new PasswordHasher();

        var users = new List<User>();

        for (var i = 0; i < persons.Count; i++)
        {
            var accountName = i == 0 ? AdminAccountName : "user" + i.ToString("00");

            users.Add(new User(accountName, password.HashPassword(FakePasswordHash), persons[i].Id, accountName + "@almohtaref.local"));
        }

        return users;
    }
}