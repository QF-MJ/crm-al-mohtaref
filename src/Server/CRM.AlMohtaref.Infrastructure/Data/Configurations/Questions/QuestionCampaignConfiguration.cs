using CRM.AlMohtaref.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Questions;

public sealed class QuestionCampaignConfiguration : IEntityTypeConfiguration<QuestionCampaign>
{
    public void Configure(EntityTypeBuilder<QuestionCampaign> builder)
    {
        builder.ToTable("QuestionCampaigns");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.CampaignId, x.QuestionId }).IsUnique();

        builder.HasOne(x => x.Question)
            .WithMany(q => q.QuestionCampaigns)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Campaign)
            .WithMany(c => c.QuestionCampaigns)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

      
        builder.HasOne(x => x.OptionSet)
            .WithMany(s => s.QuestionCampaigns)
            .HasForeignKey(x => x.OptionSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
