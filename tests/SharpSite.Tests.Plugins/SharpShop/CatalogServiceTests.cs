using Moq;
using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Repositories;
using SharpSite.Plugins.SharpShop.Services;

namespace SharpSite.Tests.Plugins.SharpShop;

public class CatalogServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_DelegatesToRepository()
    {
        var categories = new[]
        {
            new Category(Guid.NewGuid(), "featured", "Featured")
        };
        var repository = new Mock<ICatalogRepository>();
        repository
            .Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
        var service = new CatalogService(repository.Object);

        var result = await service.GetCategoriesAsync();

        Assert.Same(categories, result);
        repository.Verify(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductAsync_DelegatesToRepository()
    {
        var productId = Guid.NewGuid();
        var expected = new Product(productId, Guid.NewGuid(), "SKU", "Name", 2m);
        var repository = new Mock<ICatalogRepository>();
        repository
            .Setup(x => x.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var service = new CatalogService(repository.Object);

        var result = await service.GetProductAsync(productId);

        Assert.Equal(expected, result);
        repository.Verify(x => x.GetProductAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
