using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace CRM.AlMohtaref.Infrastructure.Repositories
{
    public class LookupRepository(AppDbContext db) 
        : ILookupRepository
    {

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await db.Departments
                    .AsNoTracking()
                    .Where(d => d.Id == id) 
                    .Select(d => new DepartmentDto(d.Id, d.Name)) 
                    .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default)
        {
            return await db.Departments
                .AsNoTracking()
                .Select(d => new DepartmentDto(d.Id, d.Name))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<TagDto>> GetTagsAsync(CancellationToken ct = default)
        {
            return await db.Tags
                .AsNoTracking()
                .Select(t => new TagDto(t.Id, t.Name))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<TrackDto>> GetTracksAsync(CancellationToken ct = default)
        {
            return await db.Tracks
                .AsNoTracking()
               .Select(d => new TrackDto(d.Id, d.Name))
               .ToListAsync(ct);
        }
    }
}
