using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Notes.CreateNote;

public class CreateNoteCommandHandler(INoteRepository noteRepository, IContactRepository contactRepository,
    IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<CreateNoteCommand, Guid>
{
    public async Task<Guid> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        if (!await contactRepository.ExistsAsync(request.ContactId, cancellationToken))
        {
            throw new NotFoundException($"العميل صاحب المعرّف '{request.ContactId}' غير موجود.");
        }

        var note = new Note(
            Guid.NewGuid(),
            request.Text,
            request.ContactId,
            currentUser.UserId
        );

        noteRepository.Add(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
