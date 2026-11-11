using CRM.AlMohtaref.Application.Common.Dtos;


namespace CRM.AlMohtaref.Application.Features.Auth.Login
{
    public record LoginDto(
         string Token,
         string? RefreshToken,
         DateTime ExpiresIn,
         Guid UserId,
         string FullName,
         List<PermissionDto> Permissions
    );
}
