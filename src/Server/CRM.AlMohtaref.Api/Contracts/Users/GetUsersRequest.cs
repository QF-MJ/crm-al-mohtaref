
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Api.Contracts.Users;

public class GetUsersRequest : PagedRequest
{
    public string? Name { get; set; }

    public string? AccountName { get; set; }

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public Role? Role { get; set; }

    public Guid? DepartmentId { get; set; }

    public GetUsersQuery ToQuery() => new(
        PageNumber,
        Name,
        AccountName,
        Email,
        IsActive,
        Role,
        DepartmentId
    );
}