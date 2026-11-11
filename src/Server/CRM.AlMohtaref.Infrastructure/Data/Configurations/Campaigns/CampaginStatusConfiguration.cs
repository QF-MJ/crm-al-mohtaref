using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Campaigns
{
    public class CampaginStatusConfiguration : IEntityTypeConfiguration<CampaginStatus>
    {
        public void Configure(EntityTypeBuilder<CampaginStatus> builder)
        {
            builder.ToTable("CampaignStatuses");
            builder.HasKey(x => x.Id);

            builder.Property(n => n.Name).HasMaxLength(200);
        }


    }
}
