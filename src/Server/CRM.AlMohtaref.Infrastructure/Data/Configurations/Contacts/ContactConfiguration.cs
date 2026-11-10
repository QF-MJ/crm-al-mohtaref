using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.PhoneNumber);

        builder.HasOne(c => c.Person)
            .WithMany(p => p.Contacts)
            .HasForeignKey(c => c.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}