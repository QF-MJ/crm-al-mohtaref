using CRM.AlMohtaref.Domain.Entities.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Campaigns;

public sealed class PlatformCampaignConfiguration : IEntityTypeConfiguration<PlatformCampaign>
{
    public void Configure(EntityTypeBuilder<PlatformCampaign> builder)
    {
        builder.ToTable("PlatformCampaigns");

        builder.HasKey(p => p.Id);

      
        builder.HasIndex(p => new { p.PlatformId, p.CampaignId }).IsUnique();

        builder.HasOne(p => p.Platform)
            .WithMany(x => x.PlatformCampaigns)
            .HasForeignKey(p => p.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Campaign)
            .WithMany(c => c.PlatformCampaigns)
            .HasForeignKey(p => p.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
