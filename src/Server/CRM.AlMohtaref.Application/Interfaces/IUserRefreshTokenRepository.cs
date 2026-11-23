using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface IUserRefreshTokenRepository
    {
        void Add(UserRefreshToken token);

        Task<UserRefreshToken?> GetById(Guid Id, CancellationToken ct = default);

        Task RevokeUserTokens(Guid userId, CancellationToken ct = default);
    }
}
