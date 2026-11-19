using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities.Users;

public class UserPermission
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Role Role { get; private set; }

    public User User { get; private set; }

    public Department Department { get; private set; }

    private UserPermission()
    {
        User = null!;
        Department = null!;
        Role = Role.Employee;
    }

    public UserPermission(string department , string role)
    {
        Department = new Department(department);
        Role = role == "مدير" ? Role.Manager : Role.Employee;
    }


    public UserPermission(Guid userId, Guid departmentId, Role role)
    {
        UserId = userId;
        DepartmentId = departmentId;
        Role = role;
    }
}