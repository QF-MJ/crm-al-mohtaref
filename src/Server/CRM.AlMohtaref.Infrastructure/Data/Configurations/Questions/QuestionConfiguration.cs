using CRM.AlMohtaref.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Questions;

public sealed class QuestionEntityConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(q => q.Text)
            .IsUnique();

        builder.HasOne(q => q.Configuration)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.ConfigurationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
