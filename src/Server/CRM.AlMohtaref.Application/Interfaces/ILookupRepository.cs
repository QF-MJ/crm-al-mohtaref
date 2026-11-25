using CRM.AlMohtaref.Application.Features.Lookups.Dtos;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<LookupItemDto>> GetDepartmentsAsync(CancellationToken ct = default);

        Task<LookupItemDto?> GetDepartmentByIdAsync(Guid Id, CancellationToken ct = default);

        Task<IEnumerable<LookupItemDto>> GetTracksAsync(CancellationToken ct = default);
        
        Task<IEnumerable<LookupItemDto>> GetCampaignsAsync(CancellationToken ct = default);

        Task<IEnumerable<LookupItemDto>> GetTagsAsync(CancellationToken ct = default);



    }
}
