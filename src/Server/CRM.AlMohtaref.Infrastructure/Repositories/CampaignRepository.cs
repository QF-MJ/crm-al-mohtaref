using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Repositories;

public class CampaignRepository(AppDbContext db)
    : ICampaignRepository
{
    public async Task<bool> ExistsAsync(Guid id, string title, CancellationToken ct = default)
    {
        return await db.Campaigns
            .AnyAsync(c => c.Id != id && c.Title == title, ct);
    }

    public async Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Campaigns
             .Include(c => c.PlatformCampaigns)
             .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
