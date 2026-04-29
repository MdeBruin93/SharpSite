namespace SharpSite.Plugin.SharpShop.Models;

public sealed record CartItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice)
{
    public decimal LineTotal => UnitPrice * Quantity;
}