using Moq;
using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Repositories;
using SharpSite.Plugins.SharpShop.Services;

namespace SharpSite.Tests.Plugins.SharpShop;

public class ShoppingCartServiceTests
{
    [Fact]
    public async Task AddItemAsync_MergesQuantityWhenProductAlreadyExists()
    {
        var cartId = "cart-merge";
        var product = new Product(Guid.NewGuid(), Guid.NewGuid(), "SKU-1", "Demo Product", 9.5m);
        var catalogRepository = new Mock<ICatalogRepository>();
        catalogRepository
            .Setup(x => x.GetProductAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        var cartRepository = new InMemoryShoppingCartRepository();
        await cartRepository.UpsertItemAsync(cartId, new CartItem(product.Id, product.Name, 1, product.Price));
        var orderRepository = new Mock<IOrderRepository>();

        var service = new ShoppingCartService(catalogRepository.Object, cartRepository, orderRepository.Object);

        var snapshot = await service.AddItemAsync(cartId, product.Id, 2);

        Assert.Single(snapshot.Items);
        Assert.Equal(3, snapshot.Items[0].Quantity);
        Assert.Equal(3, snapshot.TotalQuantity);
        Assert.Equal(28.5m, snapshot.Subtotal);
    }

    [Fact]
    public async Task AddItemAsync_InactiveProduct_ThrowsInvalidOperationException()
    {
        var productId = Guid.NewGuid();
        var inactive = new Product(productId, Guid.NewGuid(), "SKU-2", "Inactive", 9.0m, IsActive: false);
        var catalogRepository = new Mock<ICatalogRepository>();
        catalogRepository
            .Setup(x => x.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactive);
        var service = new ShoppingCartService(
            catalogRepository.Object,
            new InMemoryShoppingCartRepository(),
            new Mock<IOrderRepository>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddItemAsync("cart", productId, 1));
    }

    [Fact]
    public async Task SetQuantityAsync_Zero_RemovesItem()
    {
        var cartId = "cart-zero";
        var productId = Guid.NewGuid();
        var cartRepository = new InMemoryShoppingCartRepository();
        await cartRepository.UpsertItemAsync(cartId, new CartItem(productId, "Product", 4, 3m));

        var service = new ShoppingCartService(
            new Mock<ICatalogRepository>().Object,
            cartRepository,
            new Mock<IOrderRepository>().Object);

        var snapshot = await service.SetQuantityAsync(cartId, productId, 0);

        Assert.Empty(snapshot.Items);
        Assert.Equal(0, snapshot.TotalQuantity);
        Assert.Equal(0m, snapshot.Subtotal);
    }

    [Fact]
    public async Task CheckoutAsync_EmptyCart_ThrowsInvalidOperationException()
    {
        var service = new ShoppingCartService(
            new Mock<ICatalogRepository>().Object,
            new InMemoryShoppingCartRepository(),
            new Mock<IOrderRepository>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckoutAsync("cart-empty"));
    }

    [Fact]
    public async Task CheckoutAsync_PersistsOrder_AndClearsCart()
    {
        var cartId = "cart-checkout";
        var cartRepository = new InMemoryShoppingCartRepository();
        await cartRepository.UpsertItemAsync(cartId, new CartItem(Guid.NewGuid(), "Hat", 2, 12.5m));

        Order? savedOrder = null;
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => savedOrder = order)
            .Returns(Task.CompletedTask);

        var service = new ShoppingCartService(
            new Mock<ICatalogRepository>().Object,
            cartRepository,
            orderRepository.Object);

        var before = DateTimeOffset.UtcNow;
        var order = await service.CheckoutAsync(cartId);
        var after = DateTimeOffset.UtcNow;

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(cartId, order.CartId);
        Assert.Single(order.Items);
        Assert.Equal(25m, order.Subtotal);
        Assert.InRange(order.CreatedUtc, before, after);

        Assert.NotNull(savedOrder);
        Assert.Equal(order.Id, savedOrder!.Id);
        orderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(await cartRepository.GetItemsAsync(cartId));
    }
}
