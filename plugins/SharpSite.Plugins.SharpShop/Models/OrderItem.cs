namespace SharpSite.Plugins.SharpShop.Models;

public sealed record OrderItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice)
{
    public decimal LineTotal => UnitPrice * Quantity;
}