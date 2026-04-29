using SharpSite.Plugins.SharpShop.Models;

namespace SharpSite.Plugins.SharpShop.Repositories;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCartIdAsync(string cartId, CancellationToken cancellationToken = default);
}