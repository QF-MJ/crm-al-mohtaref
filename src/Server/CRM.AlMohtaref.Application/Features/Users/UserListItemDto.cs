namespace CRM.AlMohtaref.Application.Features.Users;

public class UserListItemDto
{
    public Guid Id { get; set; }

    public string AccountName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
