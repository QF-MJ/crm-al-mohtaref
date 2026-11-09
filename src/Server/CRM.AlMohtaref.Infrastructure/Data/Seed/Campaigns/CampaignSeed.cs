using CRM.AlMohtaref.Domain.Entities.Campaigns;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Campaigns;

// 10 campaigns (5 per course). Statuses cover every state of the state diagram.
internal sealed class CampaignSeed : EntitySeed<Campaign>
{
    private static readonly (string Title, string Track)[] Items =
    {
        ("برمجة الأطفال - الدفعة الصيفية",              TrackSeed.Programming),
        ("برمجة الأطفال - Scratch للمبتدئين",           TrackSeed.Programming),
        ("برمجة الأطفال - Python للناشئين",             TrackSeed.Programming),
        ("برمجة الأطفال - التسجيل المبكر",              TrackSeed.Programming),
        ("برمجة الأطفال - الجلسة التجريبية المجانية",   TrackSeed.Programming),
        ("إنجليزي الأطفال - التأسيس من الصفر",          TrackSeed.English),
        ("إنجليزي الأطفال - تطوير المحادثة",            TrackSeed.English),
        ("إنجليزي الأطفال - التقوية في المنهج المدرسي", TrackSeed.English),
        ("إنجليزي الأطفال - الجلسات الفردية",           TrackSeed.English),
        ("إنجليزي الأطفال - المجموعات التفاعلية",       TrackSeed.English)
    };

    public override int Order => 300;

    protected override async Task<IReadOnlyList<Campaign>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var adminId = await SeedHelpers.GetAdminUserIdAsync(db, ct);
        var trackId = await db.Set<Track>().ToDictionaryAsync(t => t.Name, t => t.Id, ct);

        var result = new List<Campaign>();

        for (var i = 0; i < Items.Length; i++)
        {
            var item = Items[i];

            result.Add(new Campaign(
                title: item.Title,
                trackId: trackId[item.Track],
                createdByUserId: adminId,
                description: "حملة تجريبية: " + item.Title,
                url: "https://docs.google.com/spreadsheets/d/seed-sheet-" + (i + 1).ToString("00"),
                budget: 500m * (i + 1)));
        }

        return result;
    }
}