using Domain.Aggregates.Users;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories;

public class RefreshTokenRepository(EShopContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default)
        => db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        => await db.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public void Update(RefreshToken refreshToken)
        => db.RefreshTokens.Update(refreshToken);
}
