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

    public ICollection<UserPermission> UserPermissions { get;  set; }

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
        AccountName = accountName;
        Email = email;
        FullName = fullName;

        if (isActive.HasValue)
            IsActive = isActive.Value;
    }

    public bool HasPermissionsChanged(IEnumerable<(Role RoleId, Guid DepartmentId)> permissions)
    {
        var requestedPermissions = permissions.ToHashSet();

        var currentPermissions = UserPermissions
            .Select(p => (p.Role, p.DepartmentId))
            .ToHashSet();

        return !currentPermissions.SetEquals(requestedPermissions);
    }


    public void SynchronizePermissions(IEnumerable<(Role RoleId, Guid DepartmentId)> permissions)
    {
        var requestedPermissions = permissions.Distinct().ToHashSet();

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
