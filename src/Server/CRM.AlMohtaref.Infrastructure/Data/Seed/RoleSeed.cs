using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// Roles come from the actor generalization page of the use case diagram:
//   Employee : موظف مبيعات / موظف تسويق / موظف خدمة العملاء / موجه أكاديمي
//   Manager  : مدير مبيعات / مدير تسويق / مدير خدمة العملاء / مدير التوجيه الأكاديمي
//   Admin    : generalizes every manager role
// "مستخدم (User)" and "مدير المهام" are abstract actors, so they are not stored as roles.
internal static class RoleSeed
{
    public const string Employee = "موظف";
    public const string Manager = "مدير";

    public static readonly string[] Names = { Employee, Manager };

    public static List<Role> Create()
    {
        return Names.Select(n => new Role(n)).ToList();
    }
}
