using CRM.AlMohtaref.Application.Features.Lookups;
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

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default)
        {
            return await _db.Departments
                .Select(d => new DepartmentDto(d.Id, d.Name))
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync(CancellationToken ct = default)
        {
            return await _db.Roles
                .Select(d => new RoleDto(d.Id, d.Name))
                .ToListAsync(ct);
        }
    }
}
