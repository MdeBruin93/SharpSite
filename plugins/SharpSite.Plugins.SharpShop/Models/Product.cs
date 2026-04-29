namespace SharpSite.Plugins.SharpShop.Models;

public sealed record Product(
    Guid Id,
    Guid CategoryId,
    string Sku,
    string Name,
    decimal Price,
    string? Description = null,
    bool IsActive = true,
    DateTimeOffset CreatedUtc = default)
{
    public DateTimeOffset CreatedUtc { get; init; } = CreatedUtc == default ? DateTimeOffset.UtcNow : CreatedUtc;
}