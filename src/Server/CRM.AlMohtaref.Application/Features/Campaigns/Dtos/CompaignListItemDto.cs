using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Features.Campaigns.Dtos
{
    public class CompaignListItemDto
    {
        public string? Title { get; private set; }

        public decimal? Budget { get; private set; }

        public TrackDto? TrackDto { get; set; }

        public string? CampaignStatus { get; private set; }

    }
}
