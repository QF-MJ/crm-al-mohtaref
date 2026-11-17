using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Api.Contracts.Users;

public class PermissionRequest
{
    public Role Role { get; set; }

    public Guid DepartmentId { get; set; }

    public PermissionCommand ToCommand() => new(Role, DepartmentId);
}