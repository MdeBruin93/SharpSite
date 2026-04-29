# Project Context

- **Owner:** Jeffrey T. Fritz
- **Project:** SharpSite — a modern, accessible CMS built with .NET 9 and Blazor
- **Stack:** .NET 9, Blazor (SSR + Interactive Server), ASP.NET Core, Entity Framework Core, PostgreSQL, Docker, Playwright, xUnit, GitHub Actions
- **Created:** 2026-03-26

## Cross-Agent Status

### River Completion: #346 P0 RCE Fix (2026-03-31T13:47)
River completed the Security P0 Remote Code Execution vulnerability fix (#346). All Newtonsoft.Json with TypeNameHandling.Auto replaced with System.Text.Json. Build clean, 47 tests pass, vulnerability eliminated. **Unblocks Kaylee's anticipatory security tests for #346–#348.**

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-26 — Anticipatory Security Tests for Issues #346, #347, #348
- **Test conventions**: Existing tests follow Arrange-Act-Assert with tab indentation, namespace = directory path, xUnit `[Fact]`/`[Theory]`, Moq for dependencies.
- **PluginManager instantiation pattern**: `new PluginManager(Mock<PluginAssemblyManager>.Object, Mock<ApplicationState>.Object, Mock<ILogger<PluginManager>>.Object)` — see `HandleUploadedPlugin.cs` for reference.
- **ZIP creation for tests**: Use `MemoryStream` + `ZipArchive` in `ZipArchiveMode.Create` with `leaveOpen: true`, then wrap with `new Plugin(memoryStream, name)`.
- **Files added**:
  - `tests/SharpSite.Tests.Web/ApplicationState/Security/SerializationSecurityTests.cs` — Issue #346: 7 tests for System.Text.Json round-trip, $type rejection, no TypeNameHandling artifacts.
  - `tests/SharpSite.Tests.Web/PluginManager/Security/ZipExtractionSecurityTests.cs` — Issue #347: 8 tests for size limits, compression ratio, path traversal, valid ZIP extraction.
  - `tests/SharpSite.Tests.Web/PluginManager/Security/ThreadSafetyTests.cs` — Issue #348: 3 tests for concurrent ApplicationState.Plugins and PluginManager static ServiceCollection access.
  - `tests/SharpSite.Tests.Plugins/ConcurrentAccessTests.cs` — Issue #348: 3 tests for concurrent PluginAssemblyManager AddAssembly/RemoveAssembly and read-while-write safety.
- **Lambda discard gotcha**: Don't use `_ =` for discards inside lambdas where `_` is already the lambda parameter (causes CS0029). Use named locals instead.
- **Central Package Management**: Test projects use `<PackageReference Include="..." />` without `Version` attribute; versions are in `Directory.Packages.props`.

### 2026-04-29 - SharpShop Focused Unit Test Coverage Added
- **Coverage shape**: Added deterministic, in-memory, no-I/O tests for SharpShop repositories and services under `tests/SharpSite.Tests.Plugins/SharpShop`.
- **Repository tests added**: `InMemoryCatalogRepositoryTests`, `InMemoryShoppingCartRepositoryTests`, and `InMemoryOrderRepositoryTests` cover seeding, filters on `IsActive`, cart upsert/remove/clear semantics, case-insensitive lookups, and order sorting by `CreatedUtc` descending.
- **Service tests added**: `CatalogServiceTests` validates delegation to `ICatalogRepository`; `ShoppingCartServiceTests` validates quantity merge behavior, inactive product rejection, quantity-zero removal, checkout empty-cart guard, order persistence callback, and cart clear after checkout.
- **Project wiring learned**: `tests/SharpSite.Tests.Plugins/SharpSite.Tests.Plugins.csproj` needs a direct `ProjectReference` to `plugins/SharpSite.Plugin.SharpShop/SharpSite.Plugin.SharpShop.csproj` for plugin test compilation.
- **Namespace collision gotcha**: Adding SharpShop introduced `SharpSite.Plugin` namespace ambiguity against `SharpSite.Plugins.Plugin`; existing tests required fully qualifying `SharpSite.Plugins.Plugin` in two files.
- **Validation result**: `dotnet test .\\tests\\SharpSite.Tests.Plugins\\SharpSite.Tests.Plugins.csproj --filter "SharpSite.Tests.Plugins.SharpShop"` passed with 19/19 tests, 0 failures.
