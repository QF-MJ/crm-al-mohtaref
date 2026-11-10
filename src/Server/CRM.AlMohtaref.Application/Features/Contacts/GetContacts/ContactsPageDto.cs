using CRM.AlMohtaref.Application.Common.Dtos;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

public record ContactsPageDto
(
    List<ContactItemDto> Items,
    PageDto Page
);
