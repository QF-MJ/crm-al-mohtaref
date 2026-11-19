using CRM.AlMohtaref.Api.Contracts.Users;
using CRM.AlMohtaref.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(ISender sender) 
    : ControllerBase
{


    [HttpPost]
    public async Task<ActionResult<int>> AddUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.AccountName,
            request.FullName,
            request.Password,
            request.Permissions?
                .Select(p => new PermissionCommand(p.RoleId, p.DepartmentId))
                .ToList()
        );

        var userId = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
    }


    [HttpGet]
    public async Task<ActionResult<UsersPageDto>> GetUsers([FromQuery] GetUsersRequest request, CancellationToken ct)
    {
        return Ok(await sender.Send(request.ToQuery(), ct));
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(Guid id, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetUserQuery(id), ct));
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        await sender.Send(request.ToCommand(id), ct);

        return NoContent();
    }
}