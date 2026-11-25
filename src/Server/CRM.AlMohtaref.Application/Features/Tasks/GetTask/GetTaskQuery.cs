using CRM.AlMohtaref.Application.Features.Tasks.Dtos;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Tasks.GetTask;

public sealed record GetTaskQuery(Guid TaskId) : IRequest<TaskDetailsDto>;
