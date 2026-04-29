# SharpShop Plugin (Bootstrap)

SharpShop is an initial standalone ecommerce plugin scaffold for SharpSite. This first cut is intentionally self-contained and focused on compile-ready domain and service foundations.

## What is included

- Domain models:
  - `Category`
  - `Product`
  - `CartItem`
  - `Order`
  - `OrderItem`
- Repository interfaces with EF-backed runtime implementations:
  - `ICatalogRepository` / `EfCatalogRepository`
  - `IShoppingCartRepository` / `EfShoppingCartRepository`
  - `IOrderRepository` / `EfOrderRepository`
- Dedicated shop persistence layer:
  - `ShopDbContext` with isolated `sharpshop` schema in the same PostgreSQL database
  - Startup initializer that creates schema objects and seeds default catalog data
- Application services:
  - `CatalogService`
  - `ShoppingCartService`
- `Configure` class implementing `IRunAtStartup` with service registration.
- Plugin manifest (`manifest.json`).

## Setup

1. Build the plugin project:

```powershell
dotnet build .\plugins\SharpSite.Plugins.SharpShop\SharpSite.Plugins.SharpShop.csproj
```

2. Package and load through the standard SharpSite plugin packaging flow when ready.

## Current capabilities

- Product and category read/write operations persisted through EF Core.
- Cart add, update quantity, remove, clear.
- Checkout flow that persists orders and clears the cart.
- Simple seeded demo catalog for local development.

## Limitations

- No customer-facing storefront UI yet (service and persistence layer only).
- No Razor UI or endpoint wiring in this bootstrap.
- No taxes, shipping, discounts, payments, inventory reservation, or fulfillment pipeline.
- No auth/ownership boundaries on cart and order access in this bootstrap.

## Requires user approval

The following host-level changes are intentionally not implemented in this task because the request requires keeping changes inside the plugin folder:

1. Wire plugin `IRunAtStartup` registration into host startup flow so `Configure.AddServicesAtStartup` is called automatically.
2. Add host routing hooks to expose storefront/cart/order endpoints or Razor components from plugin assemblies.
3. Consider introducing a dedicated plugin registration contract for ecommerce services (for example a new plugin register type or startup extensibility point).

## Roadmap

- Add migrations and deployment-time database upgrade workflow for shop schema.
- Add product search/filtering and richer catalog metadata.
- Add checkout pipeline for tax, shipping, and payment providers.
- Add admin CRUD UI for catalog management.
- Add customer storefront and order-history UI.
- Add unit tests for cart/checkout business rules and repository behavior.