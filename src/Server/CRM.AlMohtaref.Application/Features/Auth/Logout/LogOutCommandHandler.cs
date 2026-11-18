
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public class LogOutCommandHandler(IUserRefreshTokenRepository _refreshRepo) : IRequestHandler<LogOutCommand , bool>
    {
        public async Task<bool> Handle(LogOutCommand request, CancellationToken cancellationToken)
        {
            var refreshTokenArray = request.refreshToken.Split(".");

            Guid tokenId = Guid.Parse(refreshTokenArray[0]);

            string refreshToken = refreshTokenArray[1];

            UserRefreshToken? token = _refreshRepo.GetById(tokenId).Result;

            if (token == null)
            {
                throw new UnauthorizedAccessException("Refresh token not found");
            }

            bool valid = BCrypt.Net.BCrypt.Verify(refreshToken, token.RefreshTokenHash);

            if (!valid)
            {
                throw new UnauthorizedAccessException("Refresh token is not valid");
            }

            return await _refreshRepo.RevokeToken(tokenId, cancellationToken);

        }
    }
}
