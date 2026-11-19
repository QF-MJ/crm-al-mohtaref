using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Interfaces.Common
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(User user);

        DateTime GetExpireAt();
    }
}
