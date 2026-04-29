using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Repositories;

namespace SharpSite.Plugins.SharpShop.Services;

public sealed class ShoppingCartService
{
    private readonly ICatalogRepository _CatalogRepository;
    private readonly IShoppingCartRepository _ShoppingCartRepository;
    private readonly IOrderRepository _OrderRepository;

    public ShoppingCartService(
        ICatalogRepository catalogRepository,
        IShoppingCartRepository shoppingCartRepository,
        IOrderRepository orderRepository)
    {
        _CatalogRepository = catalogRepository;
        _ShoppingCartRepository = shoppingCartRepository;
        _OrderRepository = orderRepository;
    }

    public async Task<CartSnapshot> GetCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var items = await _ShoppingCartRepository.GetItemsAsync(cartId, cancellationToken);
        return ToSnapshot(cartId, items);
    }

    public async Task<CartSnapshot> AddItemAsync(
        string cartId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        var product = await _CatalogRepository.GetProductAsync(productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            throw new InvalidOperationException("Product is not available.");
        }

        var existingItems = await _ShoppingCartRepository.GetItemsAsync(cartId, cancellationToken);
        var existingItem = existingItems.FirstOrDefault(i => i.ProductId == productId);

        var mergedItem = existingItem is null
            ? new CartItem(product.Id, product.Name, quantity, product.Price)
            : existingItem with { Quantity = existingItem.Quantity + quantity };

        await _ShoppingCartRepository.UpsertItemAsync(cartId, mergedItem, cancellationToken);
        return await GetCartAsync(cartId, cancellationToken);
    }

    public async Task<CartSnapshot> SetQuantityAsync(
        string cartId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
        }

        if (quantity == 0)
        {
            await _ShoppingCartRepository.RemoveItemAsync(cartId, productId, cancellationToken);
            return await GetCartAsync(cartId, cancellationToken);
        }

        var product = await _CatalogRepository.GetProductAsync(productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            throw new InvalidOperationException("Product is not available.");
        }

        var item = new CartItem(product.Id, product.Name, quantity, product.Price);
        await _ShoppingCartRepository.UpsertItemAsync(cartId, item, cancellationToken);
        return await GetCartAsync(cartId, cancellationToken);
    }

    public async Task<CartSnapshot> RemoveItemAsync(
        string cartId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        await _ShoppingCartRepository.RemoveItemAsync(cartId, productId, cancellationToken);
        return await GetCartAsync(cartId, cancellationToken);
    }

    public Task ClearAsync(string cartId, CancellationToken cancellationToken = default)
    {
        return _ShoppingCartRepository.ClearAsync(cartId, cancellationToken);
    }

    public async Task<Order> CheckoutAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var items = await _ShoppingCartRepository.GetItemsAsync(cartId, cancellationToken);
        if (items.Count == 0)
        {
            throw new InvalidOperationException("Cannot checkout an empty cart.");
        }

        var orderItems = items
            .Select(i => new OrderItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
            .ToArray();

        var subtotal = orderItems.Sum(i => i.LineTotal);
        var order = new Order(
            Guid.NewGuid(),
            cartId,
            DateTimeOffset.UtcNow,
            orderItems,
            subtotal);

        await _OrderRepository.SaveAsync(order, cancellationToken);
        await _ShoppingCartRepository.ClearAsync(cartId, cancellationToken);

        return order;
    }

    private static CartSnapshot ToSnapshot(string cartId, IReadOnlyList<CartItem> items)
    {
        var subtotal = items.Sum(i => i.LineTotal);
        var quantity = items.Sum(i => i.Quantity);
        return new CartSnapshot(cartId, items, subtotal, quantity);
    }
}