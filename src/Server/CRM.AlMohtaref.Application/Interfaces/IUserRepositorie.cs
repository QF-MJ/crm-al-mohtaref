using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);

    Task<bool> ExistsAsync(Guid userId, string email, string accountName, CancellationToken ct);

    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<User?> GetByAccountNameAsync(string accountName, CancellationToken ct = default);

    Task<UserDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);

    Task<UsersPageDto> SearchAsync(GetUsersQuery query, CancellationToken ct = default);
}
