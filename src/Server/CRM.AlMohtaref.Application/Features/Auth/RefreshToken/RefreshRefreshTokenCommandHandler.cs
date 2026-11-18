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
    public class  RefreshRefreshTokenCommandHandler(IUserRepository _user , IUserRefreshTokenRepository _refreshToken , ITokenService _token) : IRequestHandler<RefreshRefreshTokenCommand, RefreshTokenDto>
    {
        public async Task<RefreshTokenDto> Handle(RefreshRefreshTokenCommand request , CancellationToken ct)
        {
            User? user = await _user.GetByAccountNameAsync(request.AccountName!);

            if(user == null)
            {
                throw new NotFoundException("User account name not found");
            }

            var refreshTokenRequest = request.RefreshToken!.Split(".");

            Guid Id = (Guid)refreshTokenRequest.GetValue(0)!;

            string? RequestRefreshToken = (string)refreshTokenRequest.GetValue(1)!;

            UserRefreshToken? refreshToken = await _refreshToken.GetById(Id);

            if (refreshToken == null || refreshToken.UserId != user.Id)
            {
                throw new UnauthorizedAccessException("Refresh token not found");
            }

            if (refreshToken.RefreshTokenExpiryTime > DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token expired");
            }

            if (refreshToken.RefreshTokenRevokedAt != null)
            {
                await _refreshToken.RevokeUserTokens(user.Id);
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(RequestRefreshToken, refreshToken.RefreshTokenHash);

            if (!isValid)
            {
                throw new UnauthorizedAccessException("Refresh token is not valid");
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
            var refreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
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
