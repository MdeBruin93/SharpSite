using Microsoft.EntityFrameworkCore;
using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Persistence;

namespace SharpSite.Plugins.SharpShop.Repositories;

public sealed class EfCatalogRepository : ICatalogRepository
{
    private readonly ShopDbContext _Context;

    public EfCatalogRepository(ShopDbContext context)
    {
        _Context = context;
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _Context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new Category(c.Id, c.Slug, c.Name, c.Description, c.IsActive))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _Context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new Product(
                p.Id,
                p.CategoryId,
                p.Sku,
                p.Name,
                p.Price,
                p.Description,
                p.IsActive,
                p.CreatedUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _Context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .Select(p => new Product(
                p.Id,
                p.CategoryId,
                p.Sku,
                p.Name,
                p.Price,
                p.Description,
                p.IsActive,
                p.CreatedUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _Context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
        {
            return null;
        }

        return new Product(
            product.Id,
            product.CategoryId,
            product.Sku,
            product.Name,
            product.Price,
            product.Description,
            product.IsActive,
            product.CreatedUtc);
    }

    public async Task UpsertCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        var existing = await _Context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);
        if (existing is null)
        {
            _Context.Categories.Add(new ShopCategoryEntity
            {
                Id = category.Id,
                Slug = category.Slug,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            });
        }
        else
        {
            existing.Slug = category.Slug;
            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.IsActive = category.IsActive;
        }

        await _Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        var existing = await _Context.Products.FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);
        if (existing is null)
        {
            _Context.Products.Add(new ShopProductEntity
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Sku = product.Sku,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                IsActive = product.IsActive,
                CreatedUtc = product.CreatedUtc
            });
        }
        else
        {
            existing.CategoryId = product.CategoryId;
            existing.Sku = product.Sku;
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Description = product.Description;
            existing.IsActive = product.IsActive;
            existing.CreatedUtc = product.CreatedUtc;
        }

        await _Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var entity = await _Context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _Context.Products.Remove(entity);
        await _Context.SaveChangesAsync(cancellationToken);
    }
}
