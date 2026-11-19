using CRM.AlMohtaref.Application.Features.Users;

namespace CRM.AlMohtaref.Api.Contracts.Users;

public class PermissionRequest
{
    public Guid RoleId { get; set; }

    public Guid DepartmentId { get; set; }

    public PermissionCommand ToCommand() => new(RoleId, DepartmentId);
}