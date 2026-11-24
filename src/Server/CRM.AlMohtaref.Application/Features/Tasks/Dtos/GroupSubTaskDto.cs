namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record GroupSubTaskDto(
    Guid Id,
    string StatusName,
    List<GroupContactDto> Contacts
);
