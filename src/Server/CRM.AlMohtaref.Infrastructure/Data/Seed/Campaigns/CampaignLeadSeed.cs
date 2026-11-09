using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Campaigns;

// Leads = a student that came through a campaign. Every second student has two leads.
// Runs after the contacts feature because it needs the students (Order 430).
internal sealed class CampaignLeadSeed : EntitySeed<CampaignLead>
{
    public override int Order => 430;

    protected override async Task<IReadOnlyList<CampaignLead>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var studentIds = await db.Set<Student>().OrderBy(s => s.Id).Select(s => s.Id).ToListAsync(ct);
        var campaignIds = await db.Set<Campaign>().OrderBy(c => c.Id).Select(c => c.Id).ToListAsync(ct);
        var statusIds = await db.Set<LeadStatus>().OrderBy(s => s.Id).Select(s => s.Id).ToListAsync(ct);


        var result = new List<CampaignLead>();

        for (var s = 0; s < studentIds.Count; s++)
        {
            var leadCount = 1 + (s % 2);

            for (var k = 0; k < leadCount; k++)
            {
                var campaignId = campaignIds[(s + k * 3) % campaignIds.Count];
                var statusId = statusIds[(s + k) % statusIds.Count];

                result.Add(new CampaignLead(campaignId, statusId, studentIds[s]));
            }
        }

        return result;
    }
}