using CRM.AlMohtaref.Application.Common.Attributes;
using MediatR;


using Role = CRM.AlMohtaref.Domain.Entities.Users.Role;
namespace CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;


[AuthorizePermisions(Role = Role.Manager, Department = "الإدارة")]
public record DeleteTaskCommand(Guid TaskId) : IRequest;
