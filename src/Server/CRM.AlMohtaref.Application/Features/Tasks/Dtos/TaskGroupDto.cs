namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record TaskGroupDto(
    string Title,
    List<GroupSubTaskDto> SubTasks
);
