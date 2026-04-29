using SharpSite.Plugin.SharpShop.Models;

namespace SharpSite.Plugin.SharpShop.Repositories;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCartIdAsync(string cartId, CancellationToken cancellationToken = default);
}