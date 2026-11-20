
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Logout
{
    public class LogoutCommandHandler(IUserRefreshTokenRepository refreshRepo , IPasswordHasher passwordHasher, IUnitOfWork unitOfWork) 
        : IRequestHandler<LogoutCommand>
    {
        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var refreshTokenArray = request.refreshToken.Split(".");

            Guid tokenId = Guid.Parse(refreshTokenArray[0]);

            string refreshToken = refreshTokenArray[1];

            UserRefreshToken? token = refreshRepo.GetById(tokenId).Result;

            if (token == null)
            {
                throw new NotFoundException("Refresh token not found");
            }

            bool valid = passwordHasher.VerifyPassword(refreshToken, token.RefreshTokenHash!);

            if (!valid)
            {
                throw new UnauthorizedException("Refresh token is not valid");
            }

            token.RevokeToken(DateTime.UtcNow);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
