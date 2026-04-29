using SharpSite.Plugins.SharpShop.Models;

namespace SharpSite.Plugins.SharpShop.Repositories;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task UpsertCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task UpsertProductAsync(Product product, CancellationToken cancellationToken = default);
    Task RemoveProductAsync(Guid productId, CancellationToken cancellationToken = default);
}