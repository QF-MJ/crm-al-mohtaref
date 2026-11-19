using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Features.Campaigns.Dtos
{
    public class CampaignListItemDto
    {
        public string? Title { get; private set; }

        public decimal? Budget { get; private set; }

        public string? TrackName { get; set; }

        public string? CampaignStatus { get; private set; }

        public CampaignListItemDto(string? title, decimal? budget, string? trackDto, string? campaignStatus)
        {
            Title = title;
            Budget = budget;
            TrackName = trackDto;
            CampaignStatus = campaignStatus;
        }
    }
}
