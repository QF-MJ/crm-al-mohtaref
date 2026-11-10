using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts;

public sealed class ContactTagConfiguration : IEntityTypeConfiguration<ContactTag>
{
    public void Configure(EntityTypeBuilder<ContactTag> builder)
    {
        // a tag belongs to exactly one of: student or contact
        builder.ToTable("ContactTags", t => t.HasCheckConstraint(
            "CK_ContactTags_Target",
            "([StudentId] IS NOT NULL AND [ContactId] IS NULL) OR ([StudentId] IS NULL AND [ContactId] IS NOT NULL)"));

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TagId, x.StudentId, x.ContactId }).IsUnique();

        builder.HasOne(x => x.Tag)
            .WithMany(t => t.ContactTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.ContactTags)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Contact)
            .WithMany(c => c.ContactTags)
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}