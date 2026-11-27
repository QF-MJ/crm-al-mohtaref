using System.Linq;

namespace CRM.AlMohtaref.Domain.Entities.Users;

public class User : Person
{
    public Guid Id { get; private set; }

    public string AccountName { get; private set; }

    public string PasswordHash { get; private set; }

    public bool IsActive { get; private set; }

    public string Email { get; private set; }

    public int FailedLoginCount { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    public ICollection<UserRefreshToken> userRefreshTokens { get; private set; }

    public ICollection<UserPermission> UserPermissions { get; private set; }

    private User()
    {
        AccountName = string.Empty;
        PasswordHash = string.Empty;
        Email = string.Empty;
        UserPermissions = new List<UserPermission>();
        userRefreshTokens = new List<UserRefreshToken>();
    }

    public User(Guid id, string accountName, string passwordHash, Person person, string email)
        : this()
    {
        if (id == Guid.Empty)
            throw new ArgumentException("معرّف المستخدم لا يمكن أن يكون فارغاً", nameof(id));

        if (string.IsNullOrWhiteSpace(accountName))
            throw new ArgumentException("اسم الحساب لا يمكن أن يكون فارغاً", nameof(accountName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("كلمة المرور المشفرة لا يمكن أن تكون فارغة", nameof(passwordHash));

        if (person is null)
            throw new ArgumentNullException(nameof(person), "بيانات الشخص لا يمكن أن تكون فارغة");

        Id = id;
        AccountName = accountName;
        PasswordHash = passwordHash;
        Email = email;
        FullName = person.FullName;
        CreatedByUserId = person.CreatedByUserId;
        IsActive = true;
    }

    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("كلمة المرور المشفرة لا يمكن أن تكون فارغة", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    public void RegisterFailedLogin()
    {
        FailedLoginCount++;
    }

    public void ResetFailedLogin()
    {
        FailedLoginCount = 0;
        LockoutEnd = null;
    }

    public void LockAccount(DateTime lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
    }

    public void UpdateProfile(string accountName, string email, bool? isActive, string fullName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            throw new ArgumentException("اسم الحساب لا يمكن أن يكون فارغاً", nameof(accountName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("البريد الإلكتروني لا يمكن أن يكون فارغاً", nameof(email));

        AccountName = accountName;
        Email = email;
        FullName = fullName;

        if (isActive.HasValue)
            IsActive = isActive.Value;
    }

    public bool HasPermissionsChanged(IEnumerable<(Role RoleId, Guid DepartmentId)> permissions)
    {
        if (permissions is null)
            throw new ArgumentNullException(nameof(permissions), "قائمة الصلاحيات لا يمكن أن تكون فارغة");

        var requestedPermissions = permissions.ToHashSet();

        var currentPermissions = UserPermissions
            .Select(p => (p.Role, p.DepartmentId))
            .ToHashSet();

        return !currentPermissions.SetEquals(requestedPermissions);
    }

    public void SynchronizePermissions(IEnumerable<(Role RoleId, Guid DepartmentId)> permissions)
    {
        if (permissions is null)
            throw new ArgumentNullException(nameof(permissions), "قائمة الصلاحيات لا يمكن أن تكون فارغة");

        var requestedPermissions = permissions.Distinct().ToHashSet();

        foreach (var (role, departmentId) in requestedPermissions)
        {
            if (!Enum.IsDefined(role))
                throw new ArgumentOutOfRangeException(nameof(permissions), "قيمة الدور الوظيفي غير صالحة");

            if (departmentId == Guid.Empty)
                throw new ArgumentException("معرّف القسم لا يمكن أن يكون فارغاً", nameof(permissions));
        }

        foreach (var existingPermission in UserPermissions.ToList())
        {
            var key = (existingPermission.Role, existingPermission.DepartmentId);

            if (!requestedPermissions.Remove(key))
                UserPermissions.Remove(existingPermission);
        }

        foreach (var (role, departmentId) in requestedPermissions)
        {
            UserPermissions.Add(new UserPermission(Id, departmentId, role));
        }
    }
}
