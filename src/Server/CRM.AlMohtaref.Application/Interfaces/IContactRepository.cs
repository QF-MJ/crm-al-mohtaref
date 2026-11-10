using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface IContactRepository
    {
        Task<ContactsPageDto> SearchAsync(GetContactsQuery query, CancellationToken ct);
    }
}
