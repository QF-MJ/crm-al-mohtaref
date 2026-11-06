namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class PermissionDto
{
    public int Id { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;
}
