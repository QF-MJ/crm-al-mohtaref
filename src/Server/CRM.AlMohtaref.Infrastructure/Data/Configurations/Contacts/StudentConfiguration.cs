using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts;

public sealed class StudentConfiguration : PersonConfiguration<Student>
{
    public override void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        base.Configure(builder);

        builder.HasKey(s => s.Id);

        // Contact (guardian) 1 -- * Students
        builder.HasOne(s => s.Contact)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ContactId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}