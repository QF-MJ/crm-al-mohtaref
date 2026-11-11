using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        // Person 1 -- 0..1 Student
        builder.HasOne(s => s.Person)
            .WithOne(p => p.Student)
            .HasForeignKey<Student>(s => s.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Contact (guardian) 1 -- * Students
        builder.HasOne(s => s.Contact)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ContactId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}