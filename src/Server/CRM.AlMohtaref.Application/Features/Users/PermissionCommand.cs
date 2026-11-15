using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Features.Users;

public record PermissionCommand(Role Role, Guid DepartmentId);
        