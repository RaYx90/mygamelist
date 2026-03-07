# GameList — GitHub Copilot Instructions

## Project
Responsive ASP.NET 10 Blazor Web App displaying a calendar of video game releases for the current year. Data synced daily from IGDB (Twitch API). SQL Server 2025, EF Core 10, Docker. Architecture: Clean Architecture / Hexagonal.

## Architecture Overview

```
GameList.Domain          → Entities, Value Objects, Ports — zero external deps
GameList.Application     → CQRS (MediatR), DTOs, Mappers
GameList.Infrastructure  → EF Core, IGDB HTTP Client, BackgroundService
GameList.Web             → Blazor pages/components, Minimal API endpoints, DI root
GameList.Api.Tests       → Integration tests (WebApplicationFactory)
```

## Coding Rules

### Language & Naming
- All code in **English**.
- **Type suffixes required**:
  - `GameEntity`, `PlatformEntity`, `GameReleaseEntity`
  - `DateRangeValue`, `ReleaseTypeEnum`
  - `GameReleaseDto`, `CalendarDayDto`, `PlatformDto`, `GameDetailDto`
  - `GetReleasesByMonthQuery`, `SyncGamesCommand`, `SyncGamesHandler`
  - `IgdbDataProviderAdapter`, `GameConfiguration`, `IgdbOptionsConfig`
  - `CalendarPage`, `DayCellComponent`, `ReleasesEndpoint`
  - `ReleasesEndpointTests`

### General
- Constructor injection only.
- Async/await everywhere — propagate `CancellationToken`.
- No `.Result` or `.Wait()`.
- No `static` classes except extension methods and constants.
- DTOs as C# `record` types (immutable).

### Domain
- Private/protected constructors + static `Create(...)` factory methods.
- Guard clauses for business rules — throw typed domain exceptions.
- Value Objects: immutable, equality by value.
- Zero NuGet dependencies in Domain project.

### Application
- One `Query/Command` + one `Handler` per use case.
- Manual mapping in `*Mapper` classes — **no AutoMapper**.
- No business logic in Handlers.
- No direct `DbContext` access — repositories only.

### Infrastructure
- **EF Core Fluent API only** in `IEntityTypeConfiguration<T>` — no data annotations on entities.
- Polly retry with exponential backoff for all HTTP calls.
- `PeriodicTimer` in BackgroundService.
- `IOptions<T>` pattern for all configuration.

### Web / Blazor
- `@rendermode InteractiveServer` on interactive components.
- Small, single-responsibility components.
- `ISender` (MediatR) for queries/commands.
- `TypedResults` in Minimal API endpoints.
- No business logic in components or endpoints.

### Tests
- xUnit + `WebApplicationFactory<Program>` + Testcontainers.MsSql (SQL Server 2025 container).
- `FakeGameDataProvider` as IGDB stub.
- One test class per endpoint.
- Naming: `Method_Scenario_ExpectedResult`.

## Do NOT
- Put business logic in endpoints, components, or handlers.
- Reference Infrastructure from Domain or Application.
- Use AutoMapper.
- Use EF Core data annotations on domain entities.
- Use `.Result` / `.Wait()` on async calls.
- Reference one project's DbContext from another project.
