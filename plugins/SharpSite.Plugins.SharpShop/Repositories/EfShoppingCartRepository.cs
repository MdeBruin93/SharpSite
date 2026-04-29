using Microsoft.EntityFrameworkCore;
using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Persistence;

namespace SharpSite.Plugins.SharpShop.Repositories;

public sealed class EfShoppingCartRepository : IShoppingCartRepository
{
    private readonly ShopDbContext _Context;

    public EfShoppingCartRepository(ShopDbContext context)
    {
        _Context = context;
    }

    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(string cartId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        var normalizedCartId = NormalizeCartId(cartId);

        return await _Context.CartItems
            .AsNoTracking()
            .Where(i => i.Cart.NormalizedCartId == normalizedCartId)
            .OrderBy(i => i.ProductName)
            .Select(i => new CartItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
            .ToArrayAsync(cancellationToken);
    }

    public async Task UpsertItemAsync(string cartId, CartItem item, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);
        ArgumentNullException.ThrowIfNull(item);

        var normalizedCartId = NormalizeCartId(cartId);

        var cart = await _Context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.NormalizedCartId == normalizedCartId, cancellationToken);

        if (cart is null)
        {
            cart = new ShopCartEntity
            {
                Id = Guid.NewGuid(),
                NormalizedCartId = normalizedCartId,
                Items = []
            };
            _Context.Carts.Add(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);

        if (existingItem is null)
        {
            cart.Items.Add(new ShopCartItemEntity
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }
        else
        {
            existingItem.ProductName = item.ProductName;
            existingItem.Quantity = item.Quantity;
            existingItem.UnitPrice = item.UnitPrice;
        }

        await _Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(string cartId, Guid productId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        var normalizedCartId = NormalizeCartId(cartId);
        var item = await _Context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Cart.NormalizedCartId == normalizedCartId && i.ProductId == productId, cancellationToken);

        if (item is null)
        {
            return;
        }

        _Context.CartItems.Remove(item);
        await _Context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAsync(string cartId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        var normalizedCartId = NormalizeCartId(cartId);

        var cart = await _Context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.NormalizedCartId == normalizedCartId, cancellationToken);

        if (cart is null)
        {
            return;
        }

        _Context.Carts.Remove(cart);
        await _Context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeCartId(string cartId)
    {
        return cartId.Trim().ToUpperInvariant();
    }
}
