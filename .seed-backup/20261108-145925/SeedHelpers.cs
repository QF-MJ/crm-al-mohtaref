using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Infrastructure.Data.Seed.Users;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

internal static class SeedHelpers
{
    // the user that "created" the fake data
    public static async Task<int> GetAdminUserIdAsync(AppDbContext db, CancellationToken ct)
    {
        var id = await db.Set<User>()
            .Where(u => u.AccountName == UserSeed.AdminAccountName)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        if (id == 0)
            throw new InvalidOperationException("Admin user not found. UserSeed must run before this seed.");

        return id;
    }

    public static string TableName<T>(AppDbContext db)
    {
        var entity = db.Model.FindEntityType(typeof(T))!;
        return "[" + (entity.GetSchema() ?? "dbo") + "].[" + entity.GetTableName() + "]";
    }
}