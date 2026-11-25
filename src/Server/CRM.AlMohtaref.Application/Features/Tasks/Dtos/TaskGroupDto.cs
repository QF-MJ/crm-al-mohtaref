namespace CRM.AlMohtaref.Application.Features.Tasks.DTOs;

public sealed record TaskGroupDto(
    string Title,
    List<GroupSubTaskDto> SubTasks
);
