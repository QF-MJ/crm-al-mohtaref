namespace CRM.AlMohtaref.Application.Features.Tasks.DTOs;

public sealed record GroupSubTaskDto(
    Guid Id,
    string StatusName,
    List<GroupContactDto> Contacts
);
