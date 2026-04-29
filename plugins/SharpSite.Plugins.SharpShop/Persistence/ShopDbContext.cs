using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharpSite.Abstractions.Base;

namespace SharpSite.Plugins.SharpShop.Persistence;

[RegisterPlugin(PluginServiceLocatorScope.Singleton, PluginRegisterType.DataStorage_EfContext)]
public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public ShopDbContext(IApplicationStateModel appState)
        : base(CreateOptions(appState.GetConfigurationByName(ApplicationStateKeys.ContentConnectionString)))
    {
    }

    public ShopDbContext(string connectionString)
        : base(CreateOptions(connectionString))
    {
    }

    private static DbContextOptions<ShopDbContext> CreateOptions(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ShopDbContext>();
        builder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "sharpshop");
        });
        return builder.Options;
    }

    public DbSet<ShopCategoryEntity> Categories => Set<ShopCategoryEntity>();
    public DbSet<ShopProductEntity> Products => Set<ShopProductEntity>();
    public DbSet<ShopCartEntity> Carts => Set<ShopCartEntity>();
    public DbSet<ShopCartItemEntity> CartItems => Set<ShopCartItemEntity>();
    public DbSet<ShopOrderEntity> Orders => Set<ShopOrderEntity>();
    public DbSet<ShopOrderItemEntity> OrderItems => Set<ShopOrderItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("sharpshop");

        modelBuilder.Entity<ShopCategoryEntity>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Slug).IsRequired();
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<ShopProductEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Sku).IsRequired();
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Price).HasPrecision(10, 2);
            entity.Property(x => x.CreatedUtc).HasConversion(new DateTimeOffsetConverter());
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => x.CategoryId);
        });

        modelBuilder.Entity<ShopCartEntity>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NormalizedCartId).IsRequired();
            entity.HasIndex(x => x.NormalizedCartId).IsUnique();
            entity.HasMany(x => x.Items)
                .WithOne(x => x.Cart)
                .HasForeignKey(x => x.CartEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShopCartItemEntity>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(10, 2);
            entity.HasIndex(x => new { x.CartEntityId, x.ProductId }).IsUnique();
        });

        modelBuilder.Entity<ShopOrderEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CartId).IsRequired();
            entity.Property(x => x.NormalizedCartId).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(10, 2);
            entity.Property(x => x.CreatedUtc).HasConversion(new DateTimeOffsetConverter());
            entity.HasIndex(x => x.NormalizedCartId);
            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShopOrderItemEntity>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(10, 2);
        });
    }
}

public sealed class ShopCategoryEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ShopProductEntity
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class ShopCartEntity
{
    public Guid Id { get; set; }
    public string NormalizedCartId { get; set; } = string.Empty;
    public List<ShopCartItemEntity> Items { get; set; } = [];
}

public sealed class ShopCartItemEntity
{
    public Guid Id { get; set; }
    public Guid CartEntityId { get; set; }
    public ShopCartEntity Cart { get; set; } = default!;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class ShopOrderEntity
{
    public Guid Id { get; set; }
    public string CartId { get; set; } = string.Empty;
    public string NormalizedCartId { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
    public decimal Subtotal { get; set; }
    public int Status { get; set; }
    public List<ShopOrderItemEntity> Items { get; set; } = [];
}

public sealed class ShopOrderItemEntity
{
    public Guid Id { get; set; }
    public Guid OrderEntityId { get; set; }
    public ShopOrderEntity Order { get; set; } = default!;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class DateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public DateTimeOffsetConverter()
        : base(v => v.UtcDateTime, v => v)
    {
    }
}
