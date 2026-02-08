using Infrastructure.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

public sealed class EShopContextFactory : IDesignTimeDbContextFactory<EShopContext>
{
    public EShopContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<EShopContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString(nameof(ConnectionStrings.PostgreSQL))
            ?? "Host=localhost;Database=eShopX;Username=user;Password=password";

        var options = new DbContextOptionsBuilder<EShopContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new EShopContext(options);
    }
}
