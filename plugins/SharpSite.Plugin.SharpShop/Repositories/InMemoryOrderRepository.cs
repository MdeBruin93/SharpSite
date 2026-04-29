using System.Collections.Concurrent;
using SharpSite.Plugin.SharpShop.Models;

namespace SharpSite.Plugin.SharpShop.Repositories;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _Orders = new();

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        _Orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _Orders.TryGetValue(orderId, out var order);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyList<Order>> GetByCartIdAsync(string cartId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        var orders = _Orders.Values
            .Where(o => string.Equals(o.CartId, cartId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(o => o.CreatedUtc)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Order>>(orders);
    }
}