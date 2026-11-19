using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Users;

public sealed class UserConfiguration : PersonConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {

        builder.ToTable("Users");

        base.Configure(builder);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.AccountName)
            .IsRequired()
            .HasColumnType("varchar(100)");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasColumnType("varchar(256)");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnType("varchar(500)");

        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.FailedLoginCount).IsRequired();
        builder.Property(u => u.LockoutEnd);

        builder.HasIndex(u => u.AccountName).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.UserPermissions)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.UserPermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
