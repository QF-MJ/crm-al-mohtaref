using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

internal static class SeedHelpers
{
    // the user that "created" the fake data = the first user (the admin, created from the person "مدير النظام").
    // No exception: UserSeed always runs before the seeds that call this.
    public static async Task<int> GetAdminUserIdAsync(AppDbContext db, CancellationToken ct)
    {
        return await db.Set<User>()
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);
    }

    public static string TableName<T>(AppDbContext db)
    {
        var entity = db.Model.FindEntityType(typeof(T))!;
        return "[" + (entity.GetSchema() ?? "dbo") + "].[" + entity.GetTableName() + "]";
    }
}