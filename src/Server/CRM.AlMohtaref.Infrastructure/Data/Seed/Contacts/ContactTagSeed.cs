using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Contacts;

// Tags attached to contacts and to students (each tag row belongs to exactly one of them).
internal sealed class ContactTagSeed : EntitySeed<ContactTag>
{
    private static readonly string[] StudentExtraTags = { TagSeed.HasDevice, TagSeed.AgreesOnline, TagSeed.StartNow };

    public override int Order => 420;

    protected override async Task<IReadOnlyList<ContactTag>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var tagId = await db.Set<Tag>().ToDictionaryAsync(t => t.Name, t => t.Id, ct);
        var contactIds = await db.Set<Contact>().OrderBy(c => c.Id).Select(c => c.Id).ToListAsync(ct);
        var studentIds = await db.Set<Student>().OrderBy(s => s.Id).Select(s => s.Id).ToListAsync(ct);

        var result = new List<ContactTag>();

        void ForContact(string tag, int id)
        {
            if (tagId.TryGetValue(tag, out var t))
                result.Add(ContactTag.ForContact(t, id));
        }

        void ForStudent(string tag, int id)
        {
            if (tagId.TryGetValue(tag, out var t))
                result.Add(ContactTag.ForStudent(t, id));
        }

        for (var i = 0; i < contactIds.Count; i++)
        {
            if (i % 5 == 0) ForContact(TagSeed.HotLead, contactIds[i]);
            if (i % 4 == 0) ForContact(TagSeed.AgreesOnline, contactIds[i]);
        }

        for (var j = 0; j < studentIds.Count; j++)
        {
            ForStudent(j % 2 == 0 ? TagSeed.InterestedProgramming : TagSeed.InterestedEnglish, studentIds[j]);
            ForStudent(StudentExtraTags[j % StudentExtraTags.Length], studentIds[j]);
        }

        return result;
    }
}