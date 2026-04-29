using Microsoft.EntityFrameworkCore;
using SharpSite.Plugins.SharpShop.Models;
using SharpSite.Plugins.SharpShop.Persistence;

namespace SharpSite.Plugins.SharpShop.Repositories;

public sealed class EfOrderRepository : IOrderRepository
{
    private readonly ShopDbContext _Context;

    public EfOrderRepository(ShopDbContext context)
    {
        _Context = context;
    }

    public async Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        var existing = await _Context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

        if (existing is null)
        {
            _Context.Orders.Add(ToEntity(order));
        }
        else
        {
            existing.CartId = order.CartId;
            existing.NormalizedCartId = NormalizeCartId(order.CartId);
            existing.CreatedUtc = order.CreatedUtc;
            existing.Subtotal = order.Subtotal;
            existing.Status = (int)order.Status;

            _Context.OrderItems.RemoveRange(existing.Items);
            existing.Items = order.Items
                .Select(i => new ShopOrderItemEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList();
        }

        await _Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var entity = await _Context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        return entity is null ? null : ToModel(entity);
    }

    public async Task<IReadOnlyList<Order>> GetByCartIdAsync(string cartId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cartId);

        var normalizedCartId = NormalizeCartId(cartId);

        var entities = await _Context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.NormalizedCartId == normalizedCartId)
            .OrderByDescending(o => o.CreatedUtc)
            .ToArrayAsync(cancellationToken);

        return entities.Select(ToModel).ToArray();
    }

    private static ShopOrderEntity ToEntity(Order order)
    {
        return new ShopOrderEntity
        {
            Id = order.Id,
            CartId = order.CartId,
            NormalizedCartId = NormalizeCartId(order.CartId),
            CreatedUtc = order.CreatedUtc,
            Subtotal = order.Subtotal,
            Status = (int)order.Status,
            Items = order.Items
                .Select(i => new ShopOrderItemEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList()
        };
    }

    private static Order ToModel(ShopOrderEntity entity)
    {
        return new Order(
            entity.Id,
            entity.CartId,
            entity.CreatedUtc,
            entity.Items
                .Select(i => new OrderItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
                .ToArray(),
            entity.Subtotal,
            (OrderStatus)entity.Status);
    }

    private static string NormalizeCartId(string cartId)
    {
        return cartId.Trim().ToUpperInvariant();
    }
}
