# GameList — Video Game Release Calendar

## Project Description

Responsive web app showing a calendar of video game releases for the current year (2026), filterable by platform (exclusive/multiplatform). AAA games only (indie excluded from sync).

### How It Works

- A **SyncBackgroundService** syncs daily from the **IGDB API** (Twitch) — indie games excluded.
- A **TranslationBackgroundService** translates descriptions to Spanish via **LibreTranslate** (20 games/15s, gradual, non-blocking).
- Data is persisted in **PostgreSQL 17** via **EF Core 10**.
- The **Vue 3 + Vite SPA** (served from `wwwroot`) displays a monthly calendar with games per day.
- Users can filter by platform and see if a game is exclusive or multiplatform.
- Auth via **JWT** — register (requires secret code) + login.
- Social features: favorites, purchases, groups (create/join), group insights.
- Responsive: 7-column grid on desktop, day-list on mobile.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 (C# 14) |
| Frontend | Vue 3 + Vite (SPA served from `wwwroot`) |
| Database | PostgreSQL 17 (Docker) |
| ORM | EF Core 10 (Code First + Migrations) |
| CQRS | MediatR |
| Data Source | IGDB API (Twitch) |
| Translation | LibreTranslate (self-hosted Docker container) |
| Resilience | Polly (exponential backoff retry) |
| Auth | JWT |
| Deployment | Docker / Docker Compose (2 services + external postgres/caddy) |
| Testing | xUnit + WebApplicationFactory + Testcontainers + NSubstitute |

## Architecture

Clean Architecture / Hexagonal — 4 layers:

```
src/
├── GameList.Domain/          → Entities, Value Objects, Ports (interfaces) — zero deps
├── GameList.Application/     → Use cases (Queries, Commands, DTOs, Mappers)
├── GameList.Infrastructure/  → Adapters (EF Core, IGDB client, Background jobs)
└── GameList.Web/             → Vue SPA + Minimal API endpoints, DI root
tests/
└── GameList.Api.Tests/       → Integration + unit tests (WebApplicationFactory)
```

### Project References

```
Domain         ← (no dependencies)
Application    ← Domain
Infrastructure ← Application, Domain
Web            ← Application, Infrastructure  (composition root)
Api.Tests      ← Web
```

### Docker Compose Services

| Service | Image | Description |
|---|---|---|
| `translate` | `libretranslate/libretranslate` | Self-hosted translation (en→es), CPU limit 2.0 |
| `web` | Local build | ASP.NET Core + Vue SPA, internal port 1080 |

**Servicios externos** (sus propios repos/compose):
| Service | Repo | Red externa |
|---|---|---|
| `postgres` | `postgres` | `postgres` |
| `caddy-proxy` | `caddy-proxy` | `caddy` |

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
| Minimal API Endpoint | `Endpoint` | `ReleasesEndpoint` |
| Test class | `Tests` | `ReleasesEndpointTests` |

## Project Rules

### General
- **After every change**: build .NET (`dotnet build`), build frontend (`npm run build`), rebuild and restart containers (`docker-compose build --no-cache web && docker-compose up -d web`). Always ensure the user sees the changes deployed.
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
- Minimal API endpoints grouped with `MapGroup`.
- No business logic in endpoints — only MediatR dispatch.
- `TypedResults` for endpoint responses.
- Vue 3 SPA built with Vite, output copied to `wwwroot`.

### Tests
- Integration + unit tests (109 total, all passing — 44 integration + 65 unit).
- One test class per endpoint/domain area.
- Arrange-Act-Assert pattern.
- Tests are independent — no shared mutable state.
- All endpoints require JWT — tests must authenticate via `IAsyncLifetime`.
- `CustomWebApplicationFactory`: PostgreSQL via Testcontainers, `FakeGameDataProvider`, hosted services removed.
- Unit tests use **NSubstitute 5.3.0**.

## Test User for Manual Testing

| Campo | Valor |
|---|---|
| Username | Claude |
| Email | claude@gamelist.com |
| Password | ClaudeTest2026! |
| Registration Code | (ver `REGISTRATION_SECRET_CODE` en `.env`) |

Usar estas credenciales para pruebas manuales con Chrome DevTools MCP.

## Additional Instructions

- GitHub Copilot instructions: `.github/copilot-instructions.md` — coding rules, naming conventions, architecture constraints. Keep in sync with this file.

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
- [x] Solution scaffold (sln + projects)
- [x] Domain layer
- [x] Application layer
- [x] Infrastructure — EF Core + PostgreSQL 17
- [x] Infrastructure — IGDB client (indie excluded)
- [x] Infrastructure — Background sync (SyncBackgroundService)
- [x] Infrastructure — Translation (TranslationBackgroundService, LibreTranslate)
- [x] Auth — JWT (register with secret code + login)
- [x] Social — favorites, purchases, groups (create/join), group insights
- [x] Web — Minimal API endpoints
- [x] Web — Vue 3 + Vite SPA
- [x] Docker & docker-compose (2 services: translate, web — db y caddy en repos externos)
- [x] Integration + unit tests (109 tests, ampliados con cobertura de edge cases y handlers)
- [x] IGDB configuration & token refresh
- [x] Security: JWT in HttpOnly cookie (gl_token) — no localStorage
- [x] Vue refactor: composables, httpClient factory, subdirectory components, no stores/

### Decisions Log
| Date | Decision |
|---|---|
| 2026-03-04 | .NET 10 GA (10.0.3), Vue 3 + Vite SPA, PostgreSQL 17, IGDB, BackgroundService native |
| 2026-03-04 | Clean Architecture (Hexagonal), English code, type suffixes |
| 2026-03-04 | EF Core 10 Code First, MediatR for CQRS, Polly for resilience |
| 2026-03-04 | xUnit + WebApplicationFactory + Testcontainers.PostgreSql + NSubstitute for tests |
| 2026-03-06 | LibreTranslate self-hosted for Spanish translations (TranslationBackgroundService, 20 games/15s) |
| 2026-03-09 | Indie games excluded from IGDB sync and removed from frontend |
| 2026-03-10 | Social features: favorites, purchases, groups (create/join), group insights |
| 2026-03-13 | 69 tests passing (39 integration + 30 unit) |
| 2026-03-15 | ~85 tests passing (44 integration + ~41 unit) — ampliados: auth logout/me, social edge cases, unit tests handlers favorites/purchases/currentUser/changeUsername |
| 2026-03-15 | GroupPage click-to-detail: clicar juego en Miembros o Coincidencias abre GameDetailModal vía GET /api/games/{id} |
| 2026-03-15 | Fix GroupPage modal: contadores ❤️/✅ del grupo siempre sincronizados — reloadGroupData resetea si el juego ya no es coincidencia, y openGameDetail busca en insights cargados si no se pasa insightEntry |
| 2026-03-13 | JWT moved from localStorage to HttpOnly cookie gl_token — /me + /logout endpoints added |
| 2026-03-13 | Vue refactored: useCalendar, useGameStatus, useGameSocialData, useFormatDate composables; httpClient.js factory; components/ reorganized in calendar/, game/, filters/ subdirs |
| 2026-03-13 | All backend error messages translated to Spanish |
| 2026-03-14 | Caddy reverse proxy with DuckDNS TLS — web internal port 1080, Caddy exposes 1443 |
| 2026-03-15 | Infraestructura extraída: PostgreSQL y Caddy a repos independientes (postgres, caddy-proxy) — redes externas postgres y caddy |
| 2026-03-16 | Redes Docker renombradas: postgres-net → postgres, caddy-net → caddy (nombres globales sin sufijo) |
| 2026-03-16 | 109 tests passing — 44 integration + 65 unit |
| 2026-03-14 | Cookie auth: SameSite=Lax, Secure based on request.IsHttps, ForwardedHeaders middleware |
| 2026-03-14 | GroupPage redesign: tabs (Members/Insights), accordion, compact topbar, copy invite code |
| 2026-03-14 | Show/hide password toggle on Login and Register pages |
| 2026-03-14 | GameDetailModal: badge de categoría (tipo de juego), botón ← volver al listado del día |
| 2026-03-14 | Build pipeline: siempre dotnet build + npm run build + docker-compose build/up tras cambios |
| 2026-03-14 | Contadores favoritos/compras se actualizan en tiempo real via CustomEvent game-status-changed |
| 2026-03-14 | Favicon emoji 🎮, título en español, lang="es" |
| 2026-03-14 | Panel favoritos/compras: botón ← volver al menú + ✕ cerrar |
| 2026-03-14 | Panel favoritos/compras: botón × en cada item para quitar directamente — contador sincronizado vía game-status-changed (sin doble decremento) |
| 2026-03-14 | Fix: Microsoft.EntityFrameworkCore.Relational fijado a 9.0.4 en tests para eliminar warning MSB3277 |
| 2026-03-14 | Fix: MapCategory fallback cambiado de Unknown(99) a MainGame(0) — IGDB devuelve null en game_type para muchos juegos base; 213 juegos corregidos en BD |
| 2026-03-14 | Fix: eliminado filtro indie (themes!=38) de query IGDB — etiquetado poco fiable excluía AAA como Crimson Desert; IsIndie queda como campo informativo |
| 2026-03-21 | Fix: botón "Hoy" navegaba al día 1 del mes en vez del día actual — causa raíz en onMonthChanged() y watcher de vista |
| 2026-03-21 | Nuevo composable useIsMobile.js: un solo listener resize compartido con debounce (antes 30+ listeners en DayCell) |
| 2026-03-21 | Breakpoint móvil unificado a 640px (antes 600 en DayCell vs 640 en CalendarPage) |
| 2026-03-21 | Modales (GameDetailModal, DayReleasesModal) se cierran con tecla Escape |
| 2026-03-21 | Accesibilidad: botón "+N más" en DayCell navegable por teclado (tabindex + keydown.enter) |
| 2026-03-21 | Botón "Hoy" ahora visible siempre que no estés en el día actual (antes solo al cambiar de mes) — nuevo evento go-today + goToToday() optimizado sin recarga innecesaria |
| 2026-03-21 | Avatares persistidos en servidor: endpoint POST /api/auth/avatar, volumen Docker, AvatarPath en UserEntity y UserDto |
| 2026-03-21 | Badges sociales expandibles: click para ver nombres de quién tiene favorito/comprado un juego (antes solo número) |
| 2026-03-21 | Backend: FavoritedByInGroup devuelve lista de nombres (antes solo conteo) para soportar expandibles |
| 2026-03-21 | Fix: doble botón X en búsqueda mobile — ocultar ::-webkit-search-cancel-button nativo |
| 2026-03-21 | Fix: contadores favoritos/compras del menú se refrescan al abrir el dropdown (antes solo en mount) |
| 2026-03-21 | Fix: badge expandible no se muestra si favoritedBy está vacío (solo el usuario actual lo tiene) |
| 2026-03-21 | UX: pestaña Coincidencias del grupo con badges expandibles (antes nombres en texto plano) |
| 2026-03-21 | Fix: filtrar releases IGDB con fechas placeholder — campo `human` sin coma = fecha aproximada (Q1, año, mes). Eliminados ~3900 releases falsos |
| 2026-03-21 | IUnitOfWork + transacción atómica en SyncGamesHandler — usuarios nunca ven estado intermedio durante re-sync |
| 2026-03-22 | Fix: panel favoritos/compras — click en juego navega al día exacto en vista Día (antes solo iba al mes). Evento global navigate-to-game-day |
| 2026-03-22 | Fix: contadores favoritos/compras del menú filtran solo juegos con release activo (antes incluían juegos sin fecha exacta tras filtro IGDB) |
