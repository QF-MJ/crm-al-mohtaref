namespace CRM.AlMohtaref.Application.Features.Tasks.Dtos;

public sealed record GroupContactDto(
    Guid Id,
    string FullName,
    string PhoneNumber
);
