using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Campaigns;

// Campaign statuses (the states of the campaign state diagram).
// Runs before CampaignSeed (Order 300) because every campaign points to one of these rows.
internal sealed class CampaignStatusSeed : EntitySeed<CampaginStatus>
{
    public const string Draft = "مسودة";
    public const string Active = "نشطة";
    public const string Paused = "متوقفة مؤقتاً";
    public const string Completed = "منتهية";
    public const string Cancelled = "ملغاة";

    public override int Order => 230;

    protected override Task<IReadOnlyList<CampaginStatus>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var names = new[] { Draft, Active, Paused, Completed, Cancelled };

        // CampaginStatus has a Guid key: EF generates it when the row is added
        return Result(names.Select(n => new CampaginStatus { Name = n }));
    }
}