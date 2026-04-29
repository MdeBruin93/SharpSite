using SharpSite.Plugin.SharpShop.Models;
using SharpSite.Plugin.SharpShop.Repositories;

namespace SharpSite.Plugin.SharpShop.Services;

public sealed class CatalogService
{
    private readonly ICatalogRepository _CatalogRepository;

    public CatalogService(ICatalogRepository catalogRepository)
    {
        _CatalogRepository = catalogRepository;
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return _CatalogRepository.GetCategoriesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return _CatalogRepository.GetProductsAsync(cancellationToken);
    }

    public Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return _CatalogRepository.GetProductsByCategoryAsync(categoryId, cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return _CatalogRepository.GetProductAsync(productId, cancellationToken);
    }
}