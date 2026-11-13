using CRM.AlMohtaref.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Questions;

public sealed class QuestionConfigurationConfiguration : IEntityTypeConfiguration<QuestionConfiguration>
{
    public void Configure(EntityTypeBuilder<QuestionConfiguration> builder)
    {
        builder.ToTable("QuestionConfigurations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type).IsRequired();
        builder.Property(c => c.Target).IsRequired();

        builder.HasOne(c => c.Field)
            .WithOne(f => f.Configuration)
            .HasForeignKey<QuestionConfiguration>(c => c.FieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
