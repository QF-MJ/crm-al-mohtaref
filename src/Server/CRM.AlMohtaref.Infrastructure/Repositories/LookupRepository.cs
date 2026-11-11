using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace CRM.AlMohtaref.Infrastructure.Repositories
{
    public class LookupRepository : ILookupRepository
    {

        private readonly AppDbContext _db;

        public LookupRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Departments
                    .AsNoTracking()
                    .Where(d => d.Id == id) 
                    .Select(d => new DepartmentDto(d.Id, d.Name)) 
                    .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default)
        {
            return await _db.Departments
                .AsNoTracking()
                .Select(d => new DepartmentDto(d.Id, d.Name))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<LeadStatusDto>> GetLeadStatusesAsync(CancellationToken ct = default)
        {
            return await _db.LeadStatuses
                .AsNoTracking()
               .Select(d => new LeadStatusDto(d.Id, d.Name))
               .ToListAsync(ct);
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync(CancellationToken ct = default)
        {
            return await _db.Roles
                .AsNoTracking()
                .Select(d => new RoleDto(d.Id, d.Name))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<TrackDto>> GetTracksAsync(CancellationToken ct = default)
        {
            return await _db.Tracks
                .AsNoTracking()
               .Select(d => new TrackDto(d.Id, d.Name))
               .ToListAsync(ct);
        }
    }
}
