using CRM.AlMohtaref.Application.Features.Lookups.Dtos;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default);

        Task<IEnumerable<RoleDto>> GetRolesAsync(CancellationToken ct = default);

        Task<IEnumerable<TrackDto>> GetTracksAsync(CancellationToken ct = default);

        Task<IEnumerable<LeadStatusDto>> GetLeadStatusesAsync(CancellationToken ct = default);
    }
}
