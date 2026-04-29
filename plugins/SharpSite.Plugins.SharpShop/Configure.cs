using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpSite.Abstractions.Base;
using SharpSite.Plugins.SharpShop.Persistence;
using SharpSite.Plugins.SharpShop.Repositories;
using SharpSite.Plugins.SharpShop.Services;

namespace SharpSite.Plugins.SharpShop;

/// <summary>
/// Registers SharpShop services when the host opts into plugin startup integration.
/// </summary>
public class Configure : IRunAtStartup
{
    public Task<IHostApplicationBuilder> AddServicesAtStartup(IHostApplicationBuilder app)
    {
        var connectionString = app.Configuration.GetConnectionString("SharpSite")
            ?? app.Configuration.GetConnectionString("postgresdb")
            ?? string.Empty;

        app.Services.AddDbContext<ShopDbContext>(options =>
            options.UseNpgsql(connectionString, dbOptions =>
                dbOptions.MigrationsHistoryTable("__EFMigrationsHistory", "sharpshop")));

        app.Services.AddHostedService<ShopDbInitializer>();

        app.Services.AddScoped<ICatalogRepository, EfCatalogRepository>();
        app.Services.AddScoped<IShoppingCartRepository, EfShoppingCartRepository>();
        app.Services.AddScoped<IOrderRepository, EfOrderRepository>();

        app.Services.AddScoped<CatalogService>();
        app.Services.AddScoped<ShoppingCartService>();

        return Task.FromResult(app);
    }

    public Task<IApplicationBuilder> ConfigureHttpApp(IApplicationBuilder app)
    {
        return Task.FromResult(app);
    }

    public Task RunOnInstall()
    {
        return Task.CompletedTask;
    }

    public Task RunOnUninstall()
    {
        return Task.CompletedTask;
    }

    public Task RunOnUpdate()
    {
        return Task.CompletedTask;
    }
}