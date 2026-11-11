namespace CRM.AlMohtaref.Domain.Entities.Users;

public class Role
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<UserPermission> UserPermissions { get; private set; }

    private Role()
    {
        Name = string.Empty;
        UserPermissions = new List<UserPermission>();
    }

    public Role(string name) : this()
    {
        Name = name;
    }
}