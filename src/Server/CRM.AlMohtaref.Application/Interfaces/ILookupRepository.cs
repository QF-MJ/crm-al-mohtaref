using CRM.AlMohtaref.Application.Features.Lookups;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default);

        Task<IEnumerable<RoleDto>> GetRolesAsync(CancellationToken ct = default);
    }
}
