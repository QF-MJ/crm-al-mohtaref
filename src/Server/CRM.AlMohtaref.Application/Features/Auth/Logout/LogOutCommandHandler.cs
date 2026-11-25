
using BCrypt.Net;
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
            var parts = request.refreshToken.Split('.');

            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenId))
                throw new BadRequestException("Refresh token is not valid");

            var token = await refreshRepo.GetById(tokenId, cancellationToken);

            if (token == null)
                throw new NotFoundException("Refresh token not found");

            if (!passwordHasher.VerifyPassword(parts[1], token.RefreshTokenHash!))
                throw new UnauthorizedException("Refresh token is not valid");
            token.RevokeToken(DateTime.UtcNow);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
