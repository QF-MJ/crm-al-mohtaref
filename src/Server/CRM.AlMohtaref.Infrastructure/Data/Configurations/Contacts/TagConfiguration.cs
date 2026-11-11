using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        // the column is called "Tag" in the database
        builder.Property(t => t.Name)
            .HasColumnName("Tag")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Name).IsUnique();
    }
}