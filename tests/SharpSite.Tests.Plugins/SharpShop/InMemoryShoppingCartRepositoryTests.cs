using SharpSite.Plugin.SharpShop.Models;
using SharpSite.Plugin.SharpShop.Repositories;

namespace SharpSite.Tests.Plugins.SharpShop;

public class InMemoryShoppingCartRepositoryTests
{
    [Fact]
    public async Task UpsertItem_AndGetItems_AreCaseInsensitiveByCartId()
    {
        var repository = new InMemoryShoppingCartRepository();
        var item = new CartItem(Guid.NewGuid(), "Alpha", 1, 12.0m);

        await repository.UpsertItemAsync("Cart-1", item);
        var items = await repository.GetItemsAsync("cart-1");

        Assert.Single(items);
        Assert.Equal(item.ProductId, items[0].ProductId);
    }

    [Fact]
    public async Task UpsertItem_ReplacesExistingItemForSameProduct()
    {
        var repository = new InMemoryShoppingCartRepository();
        var productId = Guid.NewGuid();

        await repository.UpsertItemAsync("cart-2", new CartItem(productId, "Beta", 1, 8.5m));
        await repository.UpsertItemAsync("cart-2", new CartItem(productId, "Beta", 4, 8.5m));

        var items = await repository.GetItemsAsync("cart-2");

        Assert.Single(items);
        Assert.Equal(4, items[0].Quantity);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsItemsOrderedByProductName()
    {
        var repository = new InMemoryShoppingCartRepository();

        await repository.UpsertItemAsync("cart-3", new CartItem(Guid.NewGuid(), "Zulu", 1, 10m));
        await repository.UpsertItemAsync("cart-3", new CartItem(Guid.NewGuid(), "Alpha", 1, 10m));

        var items = await repository.GetItemsAsync("cart-3");

        Assert.Collection(
            items,
            first => Assert.Equal("Alpha", first.ProductName),
            second => Assert.Equal("Zulu", second.ProductName));
    }

    [Fact]
    public async Task RemoveItemAndClear_RemoveItemsDeterministically()
    {
        var repository = new InMemoryShoppingCartRepository();
        var productId = Guid.NewGuid();

        await repository.UpsertItemAsync("cart-4", new CartItem(productId, "Alpha", 1, 5m));
        await repository.RemoveItemAsync("cart-4", productId);
        Assert.Empty(await repository.GetItemsAsync("cart-4"));

        await repository.UpsertItemAsync("cart-4", new CartItem(Guid.NewGuid(), "Beta", 1, 5m));
        await repository.ClearAsync("cart-4");
        Assert.Empty(await repository.GetItemsAsync("cart-4"));
    }

    [Fact]
    public async Task GetItemsAsync_WithUnknownCart_ReturnsEmpty()
    {
        var repository = new InMemoryShoppingCartRepository();

        var items = await repository.GetItemsAsync("missing");

        Assert.Empty(items);
    }
}
