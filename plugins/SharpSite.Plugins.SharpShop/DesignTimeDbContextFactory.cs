using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SharpSite.Plugins.SharpShop.Persistence;

namespace SharpSite.Plugins.SharpShop;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
{
    public ShopDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            "Host=localhost;Database=sharpsite;Username=sharpsite;Password=sharpsite;Port=5432";

        var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "sharpshop");
        });

        return new ShopDbContext(optionsBuilder.Options);
    }
}
