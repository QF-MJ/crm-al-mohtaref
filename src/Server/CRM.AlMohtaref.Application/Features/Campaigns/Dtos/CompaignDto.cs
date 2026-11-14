using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Application.Features.Campaigns.Dtos
{
    public class CompaignDto
    {
        public string? Title { get; private set; }

        public string? Description { get; private set; }

        public string? Url { get; private set; }

        public decimal? Budget { get; private set; }

        public TrackDto? TrackDto { get; set; }

        public string? CreatedByUserFullName { get; set; }

        public string? CampaignStatus { get; private set; }

        public ICollection<PlatFormDto>? Platforms { get; private set; }

    }
}
