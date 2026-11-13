using CRM.AlMohtaref.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Questions;

public sealed class OptionSetConfiguration : IEntityTypeConfiguration<OptionSet>
{
    public void Configure(EntityTypeBuilder<OptionSet> builder)
    {
        builder.ToTable("OptionSets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ContentHash)
            .IsRequired()
            .IsUnicode(false)
            .IsFixedLength()
            .HasMaxLength(64);

        builder.HasIndex(s => s.ContentHash)
            .IsUnique();

        builder.HasMany(s => s.Items)
            .WithOne(i => i.OptionSet)
            .HasForeignKey(i => i.OptionSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Items).UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
