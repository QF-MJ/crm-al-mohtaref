using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

internal static class UsersTestData
{
    public const string AdminDepartment = "الإدارة";
    public const string SalesDepartment = "المبيعات";
    public const string ValidPassword = "Str0ng@Pass";

    public static User NewUser(
        Guid? id = null,
        string accountName = "john.doe",
        string email = "john@example.com",
        string fullName = "John Doe")
        => new(id ?? Guid.NewGuid(), accountName, "stored-hash", new Person(fullName, Guid.NewGuid()), email);

    public static List<PermissionCommand> Perms(params (Role Role, Guid DepartmentId)[] items)
        => items.Select(i => new PermissionCommand(i.Role, i.DepartmentId)).ToList();

    public static CreateUserCommand ValidCreate()
        => new("john@example.com", "john.doe", "John Doe", ValidPassword,
               Perms((Role.Employee, Guid.NewGuid())));

    public static UpdateUserCommand ValidUpdate(Guid? id = null)
        => new(id ?? Guid.NewGuid(), "john.doe", "John Doe", "john@example.com", true,
               Perms((Role.Employee, Guid.NewGuid())));
}