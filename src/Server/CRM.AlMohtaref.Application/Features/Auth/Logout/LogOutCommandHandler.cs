
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public class LogOutCommandHandler(IUserRefreshTokenRepository _refreshRepo , IPasswordHasher _passwordHasher) : IRequestHandler<LogOutCommand , bool>
    {
        public async Task<bool> Handle(LogOutCommand request, CancellationToken cancellationToken)
        {
            var refreshTokenArray = request.refreshToken.Split(".");

            if (refreshTokenArray.Length != 2)
            {
                throw new BadRequestException("Refresh token is not valid");
            }

            Guid tokenId = Guid.Parse(refreshTokenArray[0]);

            string refreshToken = refreshTokenArray[1];

            UserRefreshToken? token = await _refreshRepo.GetById(tokenId);

            if (token == null)
            {
                throw new NotFoundException("Refresh token not found");
            }

            bool valid = _passwordHasher.VerifyPassword(refreshToken, token.RefreshTokenHash!);

            if (!valid)
            {
                throw new UnauthorizedException("Refresh token is not valid");
            }

            return await _refreshRepo.RevokeToken(tokenId, cancellationToken);

        }
    }
}
