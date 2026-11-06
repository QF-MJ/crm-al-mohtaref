namespace CRM.AlMohtaref.Api.Contracts.Users;

public class GetUsersRequest : PagedRequest
{
    public string? Name { get; set; }

    public string? AccountName { get; set; }

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public int? RoleId { get; set; }

    public int? DepartmentId { get; set; }
}
