using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Fseatures.Auth.RefreshToken
{
    public class  RefreshRefreshTokenCommandHandler(IUserRepository _user , IUserRefreshTokenRepository _refreshToken , ITokenService _token , IPasswordHasher _passwordHasher) : IRequestHandler<RefreshRefreshTokenCommand, RefreshTokenDto>
    {
        public async Task<RefreshTokenDto> Handle(RefreshRefreshTokenCommand request , CancellationToken ct)
        {
            var refreshTokenRequest = request.RefreshToken!.Split(".");

            Guid Id = (Guid)refreshTokenRequest.GetValue(0)!;

            string? RequestRefreshToken = (string)refreshTokenRequest.GetValue(1)!;

            UserRefreshToken? refreshToken = await _refreshToken.GetById(Id);

            if (refreshToken == null)
            {
                throw new UnauthorizedException("Refresh token not found");
            }

            User? user = await _user.GetByIdAsync(refreshToken.UserId);

            if(user == null)
            {
                throw new NotFoundException("User account name not found");
            }

            if (refreshToken.RefreshTokenExpiryTime > DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh token expired");
            }

            if (refreshToken.RefreshTokenRevokedAt != null)
            {
                await _refreshToken.RevokeUserTokens(user.Id);
            }

            bool isValid = _passwordHasher.VerifyPassword(RequestRefreshToken, refreshToken.RefreshTokenHash!);

            if (!isValid)
            {
                throw new UnauthorizedException("Refresh token is not valid");
            }

            if(!await _refreshToken.RevokeToken(refreshToken.Id, ct))
            {
                throw new BadRequestException("Some thing went wrong while revoking the refresh token");
            }

            string token = await _token.GenerateAccessToken(user);

            List<PermissionDto> permissions = user.UserPermissions
                .Select(p => new PermissionDto { Id = p.Id, RoleName = p.Role == Role.Manager ? "مدير" : "موظف", DepartmentName = p.Department.Name })
                .ToList();


            var newRefreshToken = _token.GenerateRefreshToken();

            //Store refresh token securely (hash + expiry + not revoked)
            var refreshTokenHash = _passwordHasher.HashPassword(newRefreshToken);
            Guid RefreshId = Guid.NewGuid();
            newRefreshToken = RefreshId.ToString() + "." + newRefreshToken;
            var refreshTokenExpiredAt = DateTime.UtcNow.AddDays(7);


            _refreshToken.Add(new UserRefreshToken(
                RefreshId,
                user.Id,
                refreshTokenHash,
                refreshTokenExpiredAt,
                null));


            return new RefreshTokenDto
            (
                token,
                newRefreshToken,
                _token.GetExpireAt(),
                user.Id,
                user.FullName ?? string.Empty,
                permissions
            );
        }


    }
}
