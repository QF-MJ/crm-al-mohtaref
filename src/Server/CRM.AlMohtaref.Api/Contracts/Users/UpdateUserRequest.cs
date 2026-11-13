using CRM.AlMohtaref.Application.Features.Users;

namespace CRM.AlMohtaref.Api.Contracts.Users;

public class UpdateUserRequest
{
    public string AccountName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool? IsActive { get; set; }

    public List<PermissionRequest>? Permissions { get; set; }

    public UpdateUserCommand ToCommand(Guid id) => new(
        id,
        AccountName,
        FullName,
        Email,
        IsActive,
        Permissions?.Select(p => p.ToCommand()).ToList());
}