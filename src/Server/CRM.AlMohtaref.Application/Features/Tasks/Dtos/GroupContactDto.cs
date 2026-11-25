namespace CRM.AlMohtaref.Application.Features.Tasks.DTOs;

public sealed record GroupContactDto(
    Guid Id,
    string FullName,
    string PhoneNumber
);
