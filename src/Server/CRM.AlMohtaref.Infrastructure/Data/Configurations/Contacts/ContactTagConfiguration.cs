using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts;

public sealed class ContactTagConfiguration : IEntityTypeConfiguration<ContactTag>
{
    public void Configure(EntityTypeBuilder<ContactTag> builder)
    {
        builder.ToTable("ContactTags");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TagId, x.ContactId }).IsUnique();

        builder.HasOne(x => x.Tag)
            .WithMany(t => t.ContactTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Contact)
            .WithMany(c => c.ContactTags)
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}