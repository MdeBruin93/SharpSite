using System.Collections.Concurrent;
using SharpSite.Plugin.SharpShop.Models;

namespace SharpSite.Plugin.SharpShop.Repositories;

public sealed class InMemoryShoppingCartRepository : IShoppingCartRepository
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, CartItem>> _Carts =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<CartItem>> GetItemsAsync(string cartId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        if (!_Carts.TryGetValue(cartId, out var items))
        {
            return Task.FromResult<IReadOnlyList<CartItem>>(Array.Empty<CartItem>());
        }

        return Task.FromResult<IReadOnlyList<CartItem>>(items.Values.OrderBy(i => i.ProductName).ToArray());
    }

    public Task UpsertItemAsync(string cartId, CartItem item, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);
        ArgumentNullException.ThrowIfNull(item);

        var cart = _Carts.GetOrAdd(cartId, _ => new ConcurrentDictionary<Guid, CartItem>());
        cart[item.ProductId] = item;
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string cartId, Guid productId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        if (_Carts.TryGetValue(cartId, out var cart))
        {
            cart.TryRemove(productId, out _);
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync(string cartId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);
        _Carts.TryRemove(cartId, out _);
        return Task.CompletedTask;
    }
}