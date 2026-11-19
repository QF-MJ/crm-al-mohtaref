using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaign;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaignsPage;

namespace CRM.AlMohtaref.Api.Contracts.Campaigns
{
    public class GetCampaignsRequest : PagedRequest 
    {
        public string? Title { get; set; }
        public decimal? Budget { get; set; }
        public string? TrackName { get; set; }
        public string? CampaignStatus { get; set; }

        //public GetCampaignsRequest(string? title, decimal? budget, string? trackName, string? campaignStatus)
        //{
        //    Title = title;
        //    Budget = budget;
        //    TrackName = trackName;
        //    CampaignStatus = campaignStatus;
        //}

        public GetCampaignsRequest() { }

        public GetCampaignsPageQuery ToQuery()
        {
            return new GetCampaignsPageQuery(PageNumber, Title, Budget, TrackName != null ? TrackName : null, CampaignStatus);
        }
    }
}
