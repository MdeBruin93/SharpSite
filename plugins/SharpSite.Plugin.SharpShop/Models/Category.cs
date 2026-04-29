namespace SharpSite.Plugin.SharpShop.Models;

public sealed record Category(
    Guid Id,
    string Slug,
    string Name,
    string? Description = null,
    bool IsActive = true);