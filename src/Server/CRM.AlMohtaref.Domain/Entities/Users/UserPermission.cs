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

    public UserPermission(string department, string role)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("اسم القسم لا يمكن أن يكون فارغاً", nameof(department));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("الدور الوظيفي لا يمكن أن يكون فارغاً", nameof(role));

        User = null!;
        Department = new Department(department);
        Role = role switch
        {
            "مدير" => Role.Manager,
            "موظف" => Role.Employee,
            _ => throw new ArgumentException($"الدور الوظيفي غير صالح: '{role}'", nameof(role))
        };
    }

    public UserPermission(Guid userId, Guid departmentId, Role role)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("معرّف المستخدم لا يمكن أن يكون فارغاً", nameof(userId));

        if (departmentId == Guid.Empty)
            throw new ArgumentException("معرّف القسم لا يمكن أن يكون فارغاً", nameof(departmentId));

        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role), "قيمة الدور الوظيفي غير صالحة");

        User = null!;
        Department = null!;
        UserId = userId;
        DepartmentId = departmentId;
        Role = role;
    }
}
