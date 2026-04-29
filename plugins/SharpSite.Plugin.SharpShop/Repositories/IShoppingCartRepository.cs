using SharpSite.Plugin.SharpShop.Models;

namespace SharpSite.Plugin.SharpShop.Repositories;

public interface IShoppingCartRepository
{
    Task<IReadOnlyList<CartItem>> GetItemsAsync(string cartId, CancellationToken cancellationToken = default);
    Task UpsertItemAsync(string cartId, CartItem item, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(string cartId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearAsync(string cartId, CancellationToken cancellationToken = default);
}