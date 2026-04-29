using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Repositories;

namespace SharpSite.Tests.Plugins.SharpShop;

public class InMemoryCatalogRepositoryTests
{
    [Fact]
    public async Task Constructor_SeedsDefaultActiveCatalog()
    {
        var repository = new InMemoryCatalogRepository();

        var categories = await repository.GetCategoriesAsync();
        var products = await repository.GetProductsAsync();

        Assert.Single(categories);
        Assert.Equal("Featured", categories[0].Name);

        Assert.Equal(2, products.Count);
        Assert.Collection(
            products,
            first => Assert.Equal("SharpShop Mug", first.Name),
            second => Assert.Equal("SharpShop T-Shirt", second.Name));
    }

    [Fact]
    public async Task UpsertCategory_InactiveCategory_IsExcludedFromGetCategories()
    {
        var repository = new InMemoryCatalogRepository();
        var inactiveCategory = new Category(Guid.NewGuid(), "inactive", "Inactive", IsActive: false);

        await repository.UpsertCategoryAsync(inactiveCategory);
        var categories = await repository.GetCategoriesAsync();

        Assert.DoesNotContain(categories, c => c.Id == inactiveCategory.Id);
    }

    [Fact]
    public async Task UpsertProduct_AndRemoveProduct_UpdatesQueryableState()
    {
        var repository = new InMemoryCatalogRepository();
        var category = new Category(Guid.NewGuid(), "shirts", "Shirts");
        var product = new Product(Guid.NewGuid(), category.Id, "SKU-1", "Logo Shirt", 29.99m);

        await repository.UpsertCategoryAsync(category);
        await repository.UpsertProductAsync(product);

        var byId = await repository.GetProductAsync(product.Id);
        var byCategory = await repository.GetProductsByCategoryAsync(category.Id);

        Assert.Equal(product, byId);
        Assert.Single(byCategory);
        Assert.Equal(product.Id, byCategory[0].Id);

        await repository.RemoveProductAsync(product.Id);

        Assert.Null(await repository.GetProductAsync(product.Id));
        Assert.Empty(await repository.GetProductsByCategoryAsync(category.Id));
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ExcludesInactiveProducts()
    {
        var repository = new InMemoryCatalogRepository();
        var category = new Category(Guid.NewGuid(), "mugs", "Mugs");
        var inactive = new Product(Guid.NewGuid(), category.Id, "MUG-X", "Legacy Mug", 11.0m, IsActive: false);

        await repository.UpsertCategoryAsync(category);
        await repository.UpsertProductAsync(inactive);

        var products = await repository.GetProductsByCategoryAsync(category.Id);

        Assert.Empty(products);
    }
}
