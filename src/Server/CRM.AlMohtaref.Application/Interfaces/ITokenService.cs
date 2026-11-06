using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(User user);
    }
}
