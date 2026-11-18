using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace CRM.AlMohtaref.Infrastructure.Repositories
{
    public class LookupRepository(AppDbContext db) 
        : ILookupRepository
    {
        public async Task<IEnumerable<LookupItemDto>> GetCampaignsAsync(CancellationToken ct = default)
        {
            return await db.Campaigns
                 .AsNoTracking()
                 .Select(d => new LookupItemDto(d.Id, d.Title))
                 .ToListAsync(ct);
        }

        public async Task<LookupItemDto?> GetDepartmentByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await db.Departments
                    .AsNoTracking()
                    .Where(d => d.Id == id) 
                    .Select(d => new LookupItemDto(d.Id, d.Name)) 
                    .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<LookupItemDto>> GetDepartmentsAsync(CancellationToken ct = default)
        {
            return await db.Departments
                .AsNoTracking()
                .Select(d => new LookupItemDto(d.Id, d.Name))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<LookupItemDto>> GetTagsAsync(CancellationToken ct = default)
        {
            return await db.Tags
                .AsNoTracking()
                .Select(t => new LookupItemDto(t.Id, t.Name))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<LookupItemDto>> GetTracksAsync(CancellationToken ct = default)
        {
            return await db.Tracks
                .AsNoTracking()
               .Select(d => new LookupItemDto(d.Id, d.Name))
               .ToListAsync(ct);
        }
    }
}
