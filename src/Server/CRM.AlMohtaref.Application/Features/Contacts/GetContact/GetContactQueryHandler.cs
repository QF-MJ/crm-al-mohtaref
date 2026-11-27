using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Contacts;

public sealed class GetContactQueryHandler(IContactRepository contactRepository)
    : IRequestHandler<GetContactQuery, ContactDetailDto>
{
    public async Task<ContactDetailDto> Handle(GetContactQuery request, CancellationToken ct)
    {
        var contact = await contactRepository.GetDetailAsync(request.Id, ct);

        if (contact is null)
            throw new NotFoundException($"جهة الاتصال بالمعرف {request.Id} غير موجودة");

        return contact;
    }
}
