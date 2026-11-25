using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Domain.Extensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Features.Auth.RefreshToken
{
    public class RefreshTokenCommandHandler(IUserRepository userRepository, IUserRefreshTokenRepository refreshTokenRepository 
        , ITokenService tokenRepository, IPasswordHasher passwordHasherRepository, IUnitOfWork unitOfWork) 
        : IRequestHandler<RefreshTokenCommand, RefreshTokenDto>
    {
        public async Task<RefreshTokenDto> Handle(RefreshTokenCommand request , CancellationToken ct)
        {
            var refreshTokenRequest = request.RefreshToken!.Split(".");

            if (refreshTokenRequest.Length != 2 || !Guid.TryParse(refreshTokenRequest[0], out var Id))
            {
                throw new BadRequestException("Refresh token is not valid");
            }


            string? RequestRefreshToken = (string)refreshTokenRequest[1];

            UserRefreshToken? refreshToken = await refreshTokenRepository.GetById(Id, ct);

            if (refreshToken == null)
            {
                throw new UnauthorizedException("Refresh token not found");
            }

            User? user = await userRepository.GetByIdAsync(refreshToken.UserId, ct);

            if(user == null)
            {
                throw new NotFoundException("User account name not found");
            }

            if (refreshToken.RefreshTokenRevokedAt != null)
            {
                await refreshTokenRepository.RevokeUserTokens(user.Id, ct);
                throw new UnauthorizedException("Refresh Token expired");
            }

            if (refreshToken.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh token expired");
            }

            bool isValid = passwordHasherRepository.VerifyPassword(RequestRefreshToken, refreshToken.RefreshTokenHash!);

            if (!isValid)
            {
                throw new UnauthorizedException("Refresh token is not valid");
            }

            refreshToken.RevokeToken(DateTime.UtcNow);

            string token = await tokenRepository.GenerateAccessToken(user);

            List<PermissionDto> permissions = user.UserPermissions
                .Select(p => new PermissionDto { Id = p.Id, RoleName = p.Role.ToRoleString(), DepartmentName = p.Department.Name })
                .ToList();


            var newRefreshToken = tokenRepository.GenerateRefreshToken();

            //Store refresh token securely (hash + expiry + not revoked)
            var refreshTokenHash = passwordHasherRepository.HashPassword(newRefreshToken);
            Guid RefreshId = Guid.NewGuid();
            newRefreshToken = RefreshId.ToString() + "." + newRefreshToken;
            var refreshTokenExpiredAt = DateTime.UtcNow.AddDays(7);


            refreshTokenRepository.Add(new UserRefreshToken(
                RefreshId,
                user.Id,
                refreshTokenHash,
                refreshTokenExpiredAt,
                null));

            await unitOfWork.SaveChangesAsync(ct);

            return new RefreshTokenDto
            (
                token,
                newRefreshToken,
                tokenRepository.GetExpireAt(),
                user.Id,
                user.FullName ?? string.Empty,
                permissions
            );
        }


    }
}
