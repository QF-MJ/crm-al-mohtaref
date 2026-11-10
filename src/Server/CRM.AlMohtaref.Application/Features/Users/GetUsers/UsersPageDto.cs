using CRM.AlMohtaref.Application.Common.Dtos;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class UsersPageDto
{
    public List<UserListItemDto> Items { get; set; } = new();

    public PageDto Page { get; set; }
}
