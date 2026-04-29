using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Repositories;

namespace SharpSite.Tests.Plugins.SharpShop;

public class InMemoryOrderRepositoryTests
{
    [Fact]
    public async Task SaveAndGetById_RoundTripsOrder()
    {
        var repository = new InMemoryOrderRepository();
        var order = CreateOrder("cart-a", DateTimeOffset.Parse("2026-01-01T00:00:00+00:00"));

        await repository.SaveAsync(order);
        var loaded = await repository.GetByIdAsync(order.Id);

        Assert.Equal(order, loaded);
    }

    [Fact]
    public async Task GetByCartId_IsCaseInsensitive_AndSortedByCreatedUtcDescending()
    {
        var repository = new InMemoryOrderRepository();
        var early = CreateOrder("cart-b", DateTimeOffset.Parse("2026-01-01T00:00:00+00:00"));
        var late = CreateOrder("CART-B", DateTimeOffset.Parse("2026-01-03T00:00:00+00:00"));
        var other = CreateOrder("cart-c", DateTimeOffset.Parse("2026-01-02T00:00:00+00:00"));

        await repository.SaveAsync(early);
        await repository.SaveAsync(late);
        await repository.SaveAsync(other);

        var byCartId = await repository.GetByCartIdAsync("Cart-B");

        Assert.Collection(
            byCartId,
            first => Assert.Equal(late.Id, first.Id),
            second => Assert.Equal(early.Id, second.Id));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var repository = new InMemoryOrderRepository();

        var loaded = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(loaded);
    }

    private static Order CreateOrder(string cartId, DateTimeOffset createdUtc)
    {
        return new Order(
            Guid.NewGuid(),
            cartId,
            createdUtc,
            [new OrderItem(Guid.NewGuid(), "Product", 2, 10m)],
            20m);
    }
}
