using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities.Users;

public class UserPermission
{
    public int Id { get; private set; }

    public int UserId { get; private set; }

    public int DepartmentId { get; private set; }

    public int RoleId { get; private set; }

    public User User { get; private set; }

    public Department Department { get; private set; }

    public Role Role { get; private set; }

    private UserPermission()
    {
        User = null!;
        Department = null!;
        Role = null!;
    }

    public UserPermission(int userId, int departmentId, int roleId)
    {
        UserId = userId;
        DepartmentId = departmentId;
        RoleId = roleId;
    }
}