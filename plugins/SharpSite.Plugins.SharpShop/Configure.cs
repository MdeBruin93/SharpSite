using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpSite.Abstractions.Base;
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
        app.Services.AddSingleton<ICatalogRepository, InMemoryCatalogRepository>();
        app.Services.AddSingleton<IShoppingCartRepository, InMemoryShoppingCartRepository>();
        app.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

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