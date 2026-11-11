using CRM.AlMohtaref.Domain.Entities.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Campaigns;

public sealed class CampaignLeadConfiguration : IEntityTypeConfiguration<CampaignLead>
{
    public void Configure(EntityTypeBuilder<CampaignLead> builder)
    {
        builder.ToTable("CampaignLeads");

        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.Campaign)
            .WithMany(c => c.Leads)
            .HasForeignKey(l => l.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Status)
            .WithMany(s => s.Leads)
            .HasForeignKey(l => l.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Student)
            .WithMany(s => s.CampaignLeads)
            .HasForeignKey(l => l.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}