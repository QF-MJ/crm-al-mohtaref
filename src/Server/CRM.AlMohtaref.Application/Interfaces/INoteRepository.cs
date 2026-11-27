using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Application.Interfaces;

public interface INoteRepository
{
    void Add(Note note);
}
