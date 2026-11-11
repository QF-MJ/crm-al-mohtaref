
using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }

        string? Email { get; }

        bool IsAuthenticated { get; }

        List<UserPermission>? Permissions { get; }

        bool HasDepartment(string? department);

        bool HasAnyDepartment(IEnumerable<PermissionDto>? permissions);
    }
}
