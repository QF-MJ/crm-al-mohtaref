using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;
using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Users
{
    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {

        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.RefreshTokenHash)
                .HasMaxLength(512)
                .IsRequired(false);

            builder.Property(rt => rt.UserId).IsRequired(true);

            builder.Property(rt => rt.RefreshTokenExpiryTime).IsRequired(true);


            builder.HasOne<User>(rt => rt.User)
                .WithMany(user => user.userRefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
