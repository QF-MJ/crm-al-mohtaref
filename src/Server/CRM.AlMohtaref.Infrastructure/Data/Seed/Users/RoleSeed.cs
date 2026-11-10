using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Users;

// Roles come from the actor generalization page of the use case diagram:
//   Employee : موظف مبيعات / موظف تسويق / موظف خدمة العملاء / موجه أكاديمي
//   Manager  : مدير مبيعات / مدير تسويق / مدير خدمة العملاء / مدير التوجيه الأكاديمي
//   Admin    : generalizes every manager role
// "مستخدم (User)" and "مدير المهام" are abstract actors, so they are not stored as roles.
internal sealed class RoleSeed : EntitySeed<Role>
{
    public const string Employee = "موظف";
    public const string Manager = "مدير";

    public override int Order => 110;

    protected override Task<IReadOnlyList<Role>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var names = new[] { Employee, Manager };
        return Result(names.Select(n => new Role(n)));
    }
}