using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts;

public sealed class ContactConfiguration : PersonConfiguration<Contact>
{
    public override void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        base.Configure(builder);

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.PhoneNumber);
    }
}