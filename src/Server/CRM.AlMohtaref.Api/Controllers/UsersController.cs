using CRM.AlMohtaref.Api.Contracts.Users;
using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Features.Users.AddUser;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<int>> AddUser([FromBody] AddUserRequest request, CancellationToken ct)
    {
        var command = new AddUserCommand(
            request.Email,
            request.AccountName,
            request.FullName,
            request.Password,
            request.Permissions?
                .Select(p => new PermissionCommand(p.RoleId, p.DepartmentId))
                .ToList()
        );
        var userId = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
    }

    //[Authorize]
    [HttpGet]
    public async Task<ActionResult<UsersPageDto>> GetUsers([FromQuery] GetUsersRequest request, CancellationToken ct)
    {
        var query = new GetUsersQuery(
            request.PageNumber,
            request.Name,
            request.AccountName,
            request.Email,
            request.IsActive,
            request.RoleId
            ,request.DepartmentId);

        return Ok(await _sender.Send(query, ct));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(int id, CancellationToken ct)
    {
        return Ok(await _sender.Send(new GetUserQuery(id), ct));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateUserCommand(
            id,
            request.AccountName,
            request.FullName,
            request.Email,
            request.IsActive,
            request.Permissions?
                .Select( p => new PermissionCommand( p.RoleId , p.DepartmentId) )
                .ToList() 
            ) , ct);

        return NoContent();
    }
}
