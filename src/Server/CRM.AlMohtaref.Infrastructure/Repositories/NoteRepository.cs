using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Infrastructure.Data; 

namespace CRM.AlMohtaref.Infrastructure.Repositories;

public class NoteRepository(AppDbContext db) 
    : INoteRepository
{
    public void Add(Note note)
    {
        db.Notes.Add(note);
    }
}
