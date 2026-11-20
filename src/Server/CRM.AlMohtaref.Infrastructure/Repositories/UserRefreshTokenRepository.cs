using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Repositories
{
    public class UserRefreshTokenRepository(AppDbContext db) : IUserRefreshTokenRepository
    {
        public void Add(UserRefreshToken token)
        {
            db.Add(token);
        }

        public async Task<UserRefreshToken?> GetById(Guid Id , CancellationToken ct = default)
        {
            return await db.userRefreshTokens.AsNoTracking().FirstOrDefaultAsync(r => r.Id == Id , ct);
        }

        public async Task<bool> RevokeToken(Guid tokenId, CancellationToken ct = default)
        {
            var token = await db.userRefreshTokens
                .FirstOrDefaultAsync(r => r.Id == tokenId, ct);

            if (token is null)
                return false;

            if (token.RefreshTokenRevokedAt is null)
            {
                token.RefreshTokenRevokedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }

            return true;
        }

        public async Task RevokeUserTokens(Guid userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            await db.userRefreshTokens
                .Where(r => r.UserId == userId && r.RefreshTokenRevokedAt == null && r.RefreshTokenExpiryTime > now)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(r => r.RefreshTokenRevokedAt, now), ct);
        }
    }
}
