using CRM.AlMohtaref.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Questions;

public sealed class OptionSetItemConfiguration : IEntityTypeConfiguration<OptionSetItem>
{
    public void Configure(EntityTypeBuilder<OptionSetItem> builder)
    {
        builder.ToTable("OptionSetItems");

        builder.HasKey(i => i.Id);

      
        builder.HasIndex(i => new { i.OptionSetId, i.OptionId }).IsUnique();


        builder.HasOne(i => i.Option)
            .WithMany(o => o.OptionSetItems)
            .HasForeignKey(i => i.OptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
