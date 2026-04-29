namespace SharpSite.Plugin.SharpShop.Models;

public sealed record Order(
    Guid Id,
    string CartId,
    DateTimeOffset CreatedUtc,
    IReadOnlyList<OrderItem> Items,
    decimal Subtotal,
    OrderStatus Status = OrderStatus.Submitted);

public enum OrderStatus
{
    Submitted,
    Paid,
    Fulfilled,
    Cancelled
}