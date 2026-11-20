using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts
{
    public class GetContactsQueryHandler(IContactRepository contact) 
        : IRequestHandler<GetContactsQuery, ContactsPageDto>
    {
        public async Task<ContactsPageDto> Handle(GetContactsQuery request, CancellationToken cancellationToken)
        {
            return await contact.SearchAsync(request, cancellationToken);
        }
    }
}
