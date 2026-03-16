using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

public sealed class EShopContextFactory : IDesignTimeDbContextFactory<EShopContext>
{
    public EShopContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Database=eshopx";

        var options = new DbContextOptionsBuilder<EShopContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new EShopContext(options);
    }
}
