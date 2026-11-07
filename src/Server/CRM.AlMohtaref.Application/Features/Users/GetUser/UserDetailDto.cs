using CRM.AlMohtaref.Application.Common.Dtos;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class UserDetailDto : UserListItemDto
{
    public List<PermissionDto> Permissions { get; set; } = new();
}
