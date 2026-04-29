using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpSite.Plugins.SharpShop.Models;

namespace SharpSite.Plugins.SharpShop.Persistence;

public sealed class ShopDbInitializer : IHostedService
{
    private readonly IServiceProvider _ServiceProvider;

    public ShopDbInitializer(IServiceProvider serviceProvider)
    {
        _ServiceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

        await context.Database.EnsureCreatedAsync(cancellationToken);
        await SeedDefaultsAsync(context, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task SeedDefaultsAsync(ShopDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Categories.AnyAsync(cancellationToken) || await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var featuredCategoryId = Guid.Parse("1c6f7d9c-1ce5-4ff7-8af8-7dfa0a22b8f6");
        context.Categories.Add(new ShopCategoryEntity
        {
            Id = featuredCategoryId,
            Slug = "featured",
            Name = "Featured",
            Description = "Starter products for SharpShop demos",
            IsActive = true
        });

        context.Products.AddRange(
            new ShopProductEntity
            {
                Id = Guid.Parse("247099e6-98aa-4a4d-a443-4f9a8a842a3f"),
                CategoryId = featuredCategoryId,
                Sku = "SHIRT-001",
                Name = "SharpShop T-Shirt",
                Price = 24.99m,
                Description = "Comfy tee with the SharpSite logo.",
                IsActive = true,
                CreatedUtc = DateTimeOffset.UtcNow
            },
            new ShopProductEntity
            {
                Id = Guid.Parse("9f0ad6dd-2c91-492f-9434-ab7786aa2f4d"),
                CategoryId = featuredCategoryId,
                Sku = "MUG-001",
                Name = "SharpShop Mug",
                Price = 14.50m,
                Description = "Ceramic mug for your build-and-test coffee.",
                IsActive = true,
                CreatedUtc = DateTimeOffset.UtcNow
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}
