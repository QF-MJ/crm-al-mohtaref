using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Campaigns;

// lead_status from the lead form data: new, contacted, registered, not interested.
internal sealed class LeadStatusSeed : EntitySeed<LeadStatus>
{
    public override int Order => 240;

    protected override Task<IReadOnlyList<LeadStatus>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var names = new[] { "جديد", "تم التواصل", "مسجل", "غير مهتم" };
        return Result(names.Select(n => new LeadStatus(n)));
    }
}