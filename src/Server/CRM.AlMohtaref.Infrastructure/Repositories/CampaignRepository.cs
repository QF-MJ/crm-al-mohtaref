using System.Linq;
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaign;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaignsPage;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Infrastructure.Data;
using CRM.AlMohtaref.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using CRM.AlMohtaref.Domain.Extensions;

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
            .Include(c => c.QuestionCampaigns)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<CampaignDto?> GetDtoByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Campaigns
             .AsNoTracking()
             .Where(c => c.Id == id)
             .Select(c => new CampaignDto(c.Id
              , c.Title
              , c.Description
              , c.Url
              , c.Budget
              , c.Track.Name
              , c.CreatedByUser.Person.FullName
              , c.CampaignStatus.ToArabicName()
              , c.PlatformCampaigns.Select(p => p.Platform.Name).ToList()))
             
             .FirstOrDefaultAsync(ct);
    }

    public async Task<List<CampaignListItemDto>> GetAll(GetCampaignsPageQuery query)
    {
        return await db.Campaigns
            .AsNoTracking()
            .Where(c => (query.Budget == null    || query.Budget == c.Budget) 
                && (query.Title == null          || query.Title == c.Title)
                && (query.CampaignStatus == null || query.CampaignStatus == c.CampaignStatus)
                && (query.TrackName == null      || query.TrackName == c.Track.Name))
            .OrderBy(c => c.Id)
            .Skip((query.PageNumber - 1) * PaginationSettings.PageSize)
            .Take(PaginationSettings.PageSize + 1)
            .Select(c => new CampaignListItemDto(c.Id
              , c.Title
              , c.Budget
              , c.Track.Name
              , c.CampaignStatus.ToArabicName()))
            .ToListAsync();
    }

    public async Task AddAsync(Campaign campaign)
    {
        await db.Campaigns.AddAsync(campaign);
        await db.SaveChangesAsync();
    }

    
    public async Task<bool> HasLeadsAsync(IEnumerable<Guid> platformCampaignIds, CancellationToken cancellationToken)
    {
        return await db.CampaignLeads
            .AnyAsync(l => platformCampaignIds.Contains(l.PlatformCampaignId), cancellationToken);
    }
}
