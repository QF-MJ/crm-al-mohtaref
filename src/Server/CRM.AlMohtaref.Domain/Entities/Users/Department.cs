namespace CRM.AlMohtaref.Domain.Entities.Users;

public class Department
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<UserPermission> UserPermissions { get; private set; }

    private Department()
    {
        Name = string.Empty;
        UserPermissions = new List<UserPermission>();
    }

    public Department(string name) : this()
    {
        Name = name;
    }
}