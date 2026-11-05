using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsAsync(int userId, string email, string accountName, CancellationToken ct);

    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<UserDetailDto?> GetDetailAsync(int id, CancellationToken ct = default);

    Task<UsersPageDto> SearchAsync(GetUsersQuery query, CancellationToken ct = default);
}
