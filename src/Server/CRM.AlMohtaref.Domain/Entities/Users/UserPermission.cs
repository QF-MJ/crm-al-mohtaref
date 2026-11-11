using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities.Users;

public class UserPermission
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid RoleId { get; private set; }

    public User User { get; private set; }

    public Department Department { get; private set; }

    public Role Role { get; private set; }

    private UserPermission()
    {
        User = null!;
        Department = null!;
        Role = null!;
    }

    public UserPermission(string department , string role)
    {
        Department = new Department(department);
        Role = new Role(role);
    }


    public UserPermission(Guid userId, Guid departmentId, Guid roleId)
    {
        UserId = userId;
        DepartmentId = departmentId;
        RoleId = roleId;
    }
}