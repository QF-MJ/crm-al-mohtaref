using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Campaigns;

// The two courses of the lead forms.
internal sealed class TrackSeed : EntitySeed<Track>
{
    public const string Programming = "البرمجة للأطفال";
    public const string English = "اللغة الإنجليزية للأطفال";

    public override int Order => 210;

    protected override Task<IReadOnlyList<Track>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var names = new[] { Programming, English };
        return Result(names.Select(n => new Track(n)));
    }
}