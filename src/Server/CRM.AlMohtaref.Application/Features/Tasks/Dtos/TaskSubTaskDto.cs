namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record TaskSubTaskDto(
    Guid Id,
    string StatusName,
    IReadOnlyList<TaskContactDto> Contacts
);
