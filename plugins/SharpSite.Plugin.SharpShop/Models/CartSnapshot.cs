namespace SharpSite.Plugin.SharpShop.Models;

public sealed record CartSnapshot(
    string CartId,
    IReadOnlyList<CartItem> Items,
    decimal Subtotal,
    int TotalQuantity);