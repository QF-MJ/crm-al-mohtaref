using CRM.AlMohtaref.Application.Features.Notes;
using CRM.AlMohtaref.Application.Features.Notes.CreateNote;

namespace CRM.AlMohtaref.Api.Contracts.Contacts;

public record CreateNoteRequest(string Text)
{
    public CreateNoteCommand ToCommand(Guid contactId) => new(contactId, Text);
}
