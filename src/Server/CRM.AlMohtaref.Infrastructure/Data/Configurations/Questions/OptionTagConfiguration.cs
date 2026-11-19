using CRM.AlMohtaref.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Questions;

public sealed class OptionTagConfiguration : IEntityTypeConfiguration<OptionTag>
{
    public void Configure(EntityTypeBuilder<OptionTag> builder)
    {
        builder.ToTable("OptionTags");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.OptionId, x.TagId }).IsUnique();

        builder.HasOne(x => x.Option)
            .WithMany(o => o.OptionTags)
            .HasForeignKey(x => x.OptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany(t => t.OptionTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
