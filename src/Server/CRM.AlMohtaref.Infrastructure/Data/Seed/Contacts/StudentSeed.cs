using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Contacts;

// Students = the children. Every student has a Person row and a guardian (Contact).
// Every third contact has two students, the others have one.
internal sealed class StudentSeed : EntitySeed<Student>
{
    private static readonly string[] FirstNames =
    {
        "ليث", "جود", "تيم", "ريان", "لين", "ماريا", "آدم", "سيلا", "كرم", "نور", "غيث", "رهف"
    };

    public override int Order => 410;

    protected override async Task<IReadOnlyList<Student>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var adminId = await SeedHelpers.GetAdminUserIdAsync(db, ct);

        var contacts = await db.Set<Contact>()
            .Include(c => c.Person)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        var persons = new List<(Person Person, Contact Contact)>();

        for (var i = 0; i < contacts.Count; i++)
        {
            // the child carries the family name of the guardian (last word of the full name)
            var familyName = contacts[i].Person.FullName!.Split(' ').Last();
            var count = i % 3 == 0 ? 2 : 1;

            for (var k = 0; k < count; k++)
            {
                var person = new Person
                {
                    FullName = FirstNames[(i * 2 + k) % FirstNames.Length] + " " + familyName,
                    CreatedByUserId = adminId
                };
                persons.Add((person, contacts[i]));
            }
        }

        // the students need the generated person ids
        db.Set<Person>().AddRange(persons.Select(x => x.Person));
        await db.SaveChangesAsync(ct);

        return persons.Select(x => new Student(x.Person.Id, x.Contact.Id)).ToList();
    }
}