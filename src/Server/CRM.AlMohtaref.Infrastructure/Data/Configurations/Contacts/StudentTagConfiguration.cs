using CRM.AlMohtaref.Domain.Entities.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Contacts
{
    public sealed class StudentTagConfiguration : IEntityTypeConfiguration<StudentTag>
    {
        public void Configure(EntityTypeBuilder<StudentTag> builder)
        {
            builder.ToTable("StudentTags");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.TagId, x.StudentId }).IsUnique();

            builder.HasOne(x => x.Tag)
                .WithMany(t => t.StudentTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(x => x.Student)
                .WithMany(c => c.StudentTags)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
