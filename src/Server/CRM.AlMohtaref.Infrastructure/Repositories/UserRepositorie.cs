using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Infrastructure.Data;
using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using CRM.AlMohtaref.Infrastructure.Services;

namespace CRM.AlMohtaref.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(int userId, string email, string accountName, CancellationToken ct)
    {
        return await _db.Users.AnyAsync(u => u.Id != userId
                                && (u.Email == email || u.AccountName == accountName));
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Users
            .Include( u => u.Person )
            .Include( u => u.UserPermissions )
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<UserDetailDto?> GetDetailAsync(int id, CancellationToken ct = default)
    {
        return await _db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDetailDto
            {
                Id = u.Id,
                AccountName = u.AccountName,
                FullName = u.Person.FullName, 
                Email = u.Email,
                IsActive = u.IsActive,
                Permissions = u.UserPermissions
                    .Select(up => new PermissionDto
                    {
                        Id = up.Id,
                        DepartmentName = up.Department.Name, 
                        RoleName = up.Role.Name
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UsersPageDto> SearchAsync(GetUsersQuery query, CancellationToken ct = default)
    {

        var rows = await _db.Users.AsNoTracking()
            .Where(u => 
                (query.IsActive == null || u.IsActive == query.IsActive)
                && (string.IsNullOrEmpty(query.Name) || u.Person.FullName.Contains(query.Name))
                && (string.IsNullOrEmpty(query.AccountName) || u.AccountName.Contains(query.AccountName))
                && (string.IsNullOrEmpty(query.Email) || u.Email.Contains(query.Email))
                    && ((query.RoleId == null && query.DepartmentId == null) 
                    || u.UserPermissions.Any(p => (query.RoleId == null || p.RoleId == query.RoleId) 
                    && (query.DepartmentId == null || p.DepartmentId == query.DepartmentId)))
            )
            .OrderBy(u => u.Id) 
            .Skip((query.PageNumber - 1) * PaginationSettings.PageSize)
            .Take(PaginationSettings.PageSize + 1)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                AccountName = u.AccountName,
                FullName = u.Person.FullName,
                Email = u.Email,
                IsActive = u.IsActive
            })
            .ToListAsync(ct);

        var hasNext = rows.Count > PaginationSettings.PageSize;
        if (hasNext)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        return new UsersPageDto
        {
            Items = rows,
            Page = new PageDto
            {
                PageNumber = query.PageNumber,
                HasNextPage = hasNext
            }
        };
    }

}
