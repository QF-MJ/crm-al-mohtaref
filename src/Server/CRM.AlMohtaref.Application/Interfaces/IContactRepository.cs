using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface IContactRepository
    {
        Task<ContactsPageDto> SearchAsync(GetContactsQuery query, CancellationToken ct);

        Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    }
}
