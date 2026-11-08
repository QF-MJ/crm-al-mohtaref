using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Users;

internal sealed class UserPermissionSeed : EntitySeed<UserPermission>
{
    public override int Order => 140;

    protected override async Task<IReadOnlyList<UserPermission>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var allDepartments = await db.Set<Department>().OrderBy(d => d.Id).ToListAsync(ct);
        var roleId = await db.Set<Role>().ToDictionaryAsync(r => r.Name, r => r.Id, ct);
        var users = await db.Set<User>().OrderBy(u => u.Id).ToListAsync(ct);

        // normal users are spread over the working departments only (not the administration)
        var departments = allDepartments.Where(d => d.Name != DepartmentSeed.Administration).ToList();
        var administration = allDepartments.First(d => d.Name == DepartmentSeed.Administration);

        var result = new List<UserPermission>();

        // users[0] is the admin: manager in the administration department
        result.Add(new UserPermission(users[0].Id, administration.Id, roleId[RoleSeed.Manager]));

        for (var i = 1; i < users.Count; i++)
        {
            // spread users over the departments; every 5th user is a manager
            var primary = departments[(i - 1) % departments.Count];
            var role = i % 5 == 0 ? RoleSeed.Manager : RoleSeed.Employee;
            result.Add(new UserPermission(users[i].Id, primary.Id, roleId[role]));

            // every 6th user also works as an employee in a second department
            if (i % 6 == 0)
            {
                var second = departments[i % departments.Count];
                result.Add(new UserPermission(users[i].Id, second.Id, roleId[RoleSeed.Employee]));
            }
        }

        return result;
    }
}