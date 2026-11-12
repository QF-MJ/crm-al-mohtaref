using CRM.AlMohtaref.Domain.Entities.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Campaigns;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.Url).HasMaxLength(500);
        builder.Property(c => c.Budget).HasPrecision(18, 2);

        builder.HasIndex(c => c.Title)
            .IsUnique();

        builder.HasOne(c => c.Track)
            .WithMany(t => t.Campaigns)
            .HasForeignKey(c => c.TrackId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CampaignStatus)
          .WithMany(cs => cs.Campaigns)
          .HasForeignKey(c => c.CampaginStatusId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}