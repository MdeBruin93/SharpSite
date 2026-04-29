using System.Collections.Concurrent;
using SharpSite.Plugins.SharpShop.Models;

namespace SharpSite.Plugins.SharpShop.Repositories;

public sealed class InMemoryCatalogRepository : ICatalogRepository
{
    private readonly ConcurrentDictionary<Guid, Category> _Categories = new();
    private readonly ConcurrentDictionary<Guid, Product> _Products = new();

    public InMemoryCatalogRepository()
    {
        SeedDefaults();
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = _Categories.Values
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Category>>(categories);
    }

    public Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = _Products.Values
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Product>>(products);
    }

    public Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var products = _Products.Values
            .Where(p => p.IsActive && p.CategoryId == categoryId)
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Product>>(products);
    }

    public Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        _Products.TryGetValue(productId, out var product);
        return Task.FromResult(product);
    }

    public Task UpsertCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        _Categories[category.Id] = category;
        return Task.CompletedTask;
    }

    public Task UpsertProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        _Products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task RemoveProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        _Products.TryRemove(productId, out _);
        return Task.CompletedTask;
    }

    private void SeedDefaults()
    {
        var featuredCategory = new Category(
            Guid.Parse("1c6f7d9c-1ce5-4ff7-8af8-7dfa0a22b8f6"),
            "featured",
            "Featured",
            "Starter products for SharpShop demos");

        _Categories[featuredCategory.Id] = featuredCategory;

        var starterProducts = new[]
        {
            new Product(
                Guid.Parse("247099e6-98aa-4a4d-a443-4f9a8a842a3f"),
                featuredCategory.Id,
                "SHIRT-001",
                "SharpShop T-Shirt",
                24.99m,
                "Comfy tee with the SharpSite logo."),
            new Product(
                Guid.Parse("9f0ad6dd-2c91-492f-9434-ab7786aa2f4d"),
                featuredCategory.Id,
                "MUG-001",
                "SharpShop Mug",
                14.50m,
                "Ceramic mug for your build-and-test coffee.")
        };

        foreach (var product in starterProducts)
        {
            _Products[product.Id] = product;
        }
    }
}