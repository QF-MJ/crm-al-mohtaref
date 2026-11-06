using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

internal static class UserPermissionSeed
{
    public static List<UserPermission> Create(
        IReadOnlyList<User> users,
        IReadOnlyList<Department> departments,
        IReadOnlyList<Role> roles)
    {
        var departmentId = departments.ToDictionary(d => d.Name, d => d.Id);
        var roleId = roles.ToDictionary(r => r.Name, r => r.Id);
        var names = DepartmentSeed.Names;

        var result = new List<UserPermission>();

        for (var i = 1; i < users.Count; i++)
        {
            // spread users over the departments; every 5th user is a manager
            var primary = names[(i - 1) % names.Length];
            var role = i % 5 == 0 ? RoleSeed.Manager : RoleSeed.Employee;
            result.Add(New(users[i].Id, departmentId[primary], roleId[role]));

            // every 6th user also works as an employee in a second department
            if (i % 6 == 0)
            {
                var second = names[i % names.Length];
                result.Add(New(users[i].Id, departmentId[second], roleId[RoleSeed.Employee]));
            }
        }

        return result;
    }

    private static UserPermission New(int userId, int departmentId, int roleId)
    {
        return new UserPermission(userId, departmentId, roleId);
    }
}
