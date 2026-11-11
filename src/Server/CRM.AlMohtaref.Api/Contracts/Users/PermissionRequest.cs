namespace CRM.AlMohtaref.Api.Contracts.Users;

public class PermissionRequest
{
    public Guid RoleId { get; set; }

    public Guid DepartmentId { get; set; }
}
