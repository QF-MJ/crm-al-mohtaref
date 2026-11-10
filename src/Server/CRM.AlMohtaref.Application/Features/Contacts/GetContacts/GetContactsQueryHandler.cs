using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts
{
    public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, ContactsPageDto>
    {
        private readonly IContactRepository _contact;

        public GetContactsQueryHandler(IContactRepository contact)
        {
            _contact = contact;
        }


        public async Task<ContactsPageDto> Handle(GetContactsQuery request, CancellationToken cancellationToken)
        {
            return await _contact.SearchAsync(request, cancellationToken);
        }
    }
}
