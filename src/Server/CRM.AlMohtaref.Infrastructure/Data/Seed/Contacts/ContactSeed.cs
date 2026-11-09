using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Contacts;

// Contacts = the guardians (parents). Each contact has a Person row (the name) and a phone number.
internal sealed class ContactSeed : EntitySeed<Contact>
{
    public const int Count = 30;

    private static readonly string[] FirstNames =
    {
        "أحمد", "محمود", "خالد", "سامي", "ياسر", "مازن", "رامي", "هيثم",
        "منى", "رغد", "سلمى", "هدى", "إيمان", "عبير", "وسام"
    };

    private static readonly string[] FamilyNames =
    {
        "الحسن", "العلي", "حداد", "قباني", "الخطيب", "درويش", "شاهين", "منصور", "السيد", "ديب"
    };

    public override int Order => 400;

    protected override async Task<IReadOnlyList<Contact>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var adminId = await SeedHelpers.GetAdminUserIdAsync(db, ct);

        var persons = new List<Person>();
        for (var i = 0; i < Count; i++)
        {
            persons.Add(new Person
            {
                FullName = FirstNames[i % FirstNames.Length] + " " + FamilyNames[(i * 3) % FamilyNames.Length],
                CreatedByUserId = adminId
            });
        }

        // the contacts need the generated person ids
        db.Set<Person>().AddRange(persons);
        await db.SaveChangesAsync(ct);

        return persons.Select((p, i) => new Contact(p.Id, Phone(i))).ToList();
    }

    // 09xxxxxxxx
    private static string Phone(int i)
    {
        return "09" + (31000000 + i * 137911).ToString("00000000");
    }
}