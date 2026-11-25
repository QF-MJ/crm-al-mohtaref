using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Questions;

namespace CRM.AlMohtaref.Application.Features.Campaigns.Dtos
{
    public class CampaignDto
    {
        public Guid Id { get; private set; }
        public string? Title { get; private set; }

        public string? Description { get; private set; }

        public string? Url { get; private set; }

        public decimal? Budget { get; private set; }

        public string? TrackName { get; set; }

        public string? CreatedByUserFullName { get; set; }

        public string? CampaignStatus { get; private set; }

        public ICollection<string>? PlatformsNames { get; private set; }

        public CampaignDto(Guid Id, string? title, string? description, string? url, decimal? budget, string? trackDto, string? createdByUserFullName, string? campaignStatus, ICollection<string>? platforms)
        {
            Title = title;
            Description = description;
            Url = url;
            Budget = budget;
            TrackName = trackDto;
            CreatedByUserFullName = createdByUserFullName;
            CampaignStatus = campaignStatus;
            PlatformsNames = platforms;
            this.Id = Id;
        }

    }
}
