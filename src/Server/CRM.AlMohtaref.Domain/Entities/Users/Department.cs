namespace CRM.AlMohtaref.Domain.Entities.Users;

public class Department
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<UserPermission> UserPermissions { get; private set; }

    private Department()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        UserPermissions = new List<UserPermission>();
    }

    public Department(string name) : this()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم القسم لا يمكن أن يكون فارغاً", nameof(name));

        Name = name;
    }
}
