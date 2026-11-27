using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Text)
            .IsRequired()
            .HasMaxLength(2000); 

        builder.HasOne(n => n.Contact)
            .WithMany(c => c.Notes)
            .HasForeignKey(n => n.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.CreatedByUser)
            .WithMany()
            .HasForeignKey(n => n.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
