# GameList — Video Game Release Calendar

## Project Description

Responsive web app showing a calendar of video game releases for the current year (2026), filterable by platform (exclusive/multiplatform). Covers all games: indie and AAA.

### How It Works

- A **BackgroundService** syncs daily from the **IGDB API** (Twitch).
- Data is persisted in **SQL Server 2025** via **EF Core 10**.
- The **Blazor Interactive Server** frontend displays a monthly calendar with games per day.
- Users can filter by platform and see if a game is exclusive or multiplatform.
- Responsive: 7-column grid on desktop, day-list on mobile.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 (C# 14) |
| Frontend | Blazor Web App (Interactive Server) |
| Database | SQL Server 2025 in Docker |
| ORM | EF Core 10 (Code First + Migrations) |
| CQRS | MediatR |
| Data Source | IGDB API (Twitch) |
| Resilience | Polly |
| Deployment | Docker / Docker Compose |
| Testing | xUnit + WebApplicationFactory |

## Architecture

Clean Architecture / Hexagonal — 4 layers:

```
src/
├── GameList.Domain/          → Entities, Value Objects, Ports (interfaces) — zero deps
├── GameList.Application/     → Use cases (Queries, Commands, DTOs, Mappers)
├── GameList.Infrastructure/  → Adapters (EF Core, IGDB client, Background jobs)
└── GameList.Web/             → Blazor pages/components, Minimal API endpoints, DI root
tests/
└── GameList.Api.Tests/       → Integration tests (WebApplicationFactory)
```

### Project References

```
Domain         ← (no dependencies)
Application    ← Domain
Infrastructure ← Application, Domain
Web            ← Application, Infrastructure  (composition root)
Api.Tests      ← Web
```

## Naming Conventions — Type Suffixes

All code in **English** with **type suffixes**:

| Type | Suffix | Example |
|---|---|---|
| Domain Entity | `Entity` | `GameEntity` |
| Value Object | `Value` | `DateRangeValue` |
| Enum | `Enum` | `ReleaseTypeEnum` |
| DTO | `Dto` | `GameReleaseDto` |
| Interface / Port | `I` + name | `IGameRepository` |
| Repository impl | (none) | `GameRepository` |
| Domain Service | `DomainService` | `ReleaseDomainService` |
| Application Service | `Service` | `GameSyncService` |
| Background Service | `BackgroundService` | `SyncBackgroundService` |
| Infrastructure Adapter | `Adapter` | `IgdbDataProviderAdapter` |
| EF Configuration | `Configuration` | `GameConfiguration` |
| Strongly-typed Options | `OptionsConfig` | `IgdbOptionsConfig` |
| MediatR Query | `Query` | `GetReleasesByMonthQuery` |
| MediatR Command | `Command` | `SyncGamesCommand` |
| MediatR Handler | `Handler` | `SyncGamesHandler` |
| Mapper | `Mapper` | `GameMapper` |
| Blazor Page | `Page` | `CalendarPage` |
| Blazor Component | `Component` | `DayCellComponent` |
| Minimal API Endpoint | `Endpoint` | `ReleasesEndpoint` |
| Test class | `Tests` | `ReleasesEndpointTests` |

## Project Rules

### General
- Follow **SOLID** strictly.
- Apply **DDD** in the Domain layer.
- Small classes, single-responsibility methods.
- No `static` except extension methods and constants.
- Constructor injection only — never service locator.
- Async/await everywhere. Propagate `CancellationToken` throughout.
- No `.Result` or `.Wait()` on async calls.

### Domain
- Entities: private/protected constructors + static factory methods.
- Business validations inside entities (Guard clauses).
- Value Objects: immutable, equality by value.
- Repository interfaces (ports) defined here.
- Zero external dependencies (no EF Core, no MediatR, nothing).

### Application
- One Handler per Query/Command.
- DTOs as C# `record` types (immutable).
- Manual mapping — no AutoMapper.
- No business logic in Handlers — delegate to Domain Services.
- Never access DbContext directly — only via repositories.

### Infrastructure
- EF Core Fluent API in separate `IEntityTypeConfiguration<T>` classes.
- No EF data annotations on entities.
- Typed HttpClient for IGDB with `IHttpClientFactory` + Polly.
- BackgroundService with `PeriodicTimer`.
- Options pattern for all configuration.

### Web
- Small, reusable Blazor components.
- Minimal API endpoints grouped with `MapGroup`.
- No business logic in endpoints or components — only MediatR dispatch.
- `TypedResults` for endpoint responses.

### Tests
- Integration tests only (for now).
- One test class per endpoint.
- Arrange-Act-Assert pattern.
- Tests are independent — no shared mutable state.

## Token & Cost Optimization

- **Read only necessary lines** — never full files unless required.
- **grep/search before read_file** to locate code.
- **Batch independent operations** in parallel.
- **No repeated context** in responses.
- **Concise responses** — skip intros and summaries.
- **Combine related changes** into one intervention to minimize premium requests.
- Check for existing similar code before creating new files.

## Progress Log

### Status
- [x] Configuration files (CLAUDE.md, agents, copilot-instructions)
- [ ] Solution scaffold (sln + projects)
- [ ] Domain layer
- [ ] Application layer
- [ ] Infrastructure — EF Core + SQL Server
- [ ] Infrastructure — IGDB client
- [ ] Infrastructure — Background sync
- [ ] Web — Minimal API endpoints
- [ ] Web — Blazor pages & components
- [ ] Docker & docker-compose
- [ ] Integration tests
- [ ] IGDB configuration & token refresh

### Decisions Log
| Date | Decision |
|---|---|
| 2026-03-04 | .NET 10 GA (10.0.3), Blazor Interactive Server, SQL Server 2025, IGDB, BackgroundService native |
| 2026-03-04 | Clean Architecture (Hexagonal), English code, type suffixes |
| 2026-03-04 | EF Core 10 Code First, MediatR for CQRS, Polly for resilience |
| 2026-03-04 | xUnit + WebApplicationFactory + Testcontainers.MsSql (SQL Server 2025) for tests |
