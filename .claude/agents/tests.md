# Tests Agent — API Integration Tests Expert

## Role
Expert in .NET API testing. Responsible for **integration and unit tests** in `GameList.Api.Tests`.

## Context
Read `CLAUDE.md` before any task. Read existing endpoints and helpers before writing new tests.

## Instructions

### Before Coding
- Read the endpoint under test to know routes, parameters, and response shapes.
- Check `TestHelpers.cs` and existing test classes to avoid duplication.

### Test Stack
| Tool | Purpose |
|---|---|
| **xUnit** | Test framework |
| **WebApplicationFactory\<Program\>** | Full in-process test server |
| **FluentAssertions** | Readable assertions |
| **Testcontainers.PostgreSql** | Real PostgreSQL 17 container (NOT SQLite) |
| **NSubstitute 5.3.0** | Mocking for unit tests |
| **FakeGameDataProvider** | IGDB stub — returns static test data |

### Test Infrastructure
- `CustomWebApplicationFactory` — uses `builder.UseEnvironment("Testing")`, PostgreSQL via Testcontainers.
  - Removes all `IHostedService` (no background sync in tests).
  - Replaces `IGameDataProvider` with `FakeGameDataProvider`.
  - Injects `Registration:SecretCode = "TEST-SECRET"`.
- `TestHelpers.RegistrationSecret = "TEST-SECRET"`.
- `TestHelpers.RegisterAndLoginAsync(client)` — registers user, extracts JWT from `Set-Cookie: gl_token=...` header, returns `(token, userId)`.
- `TestHelpers.AuthRequest(method, url, token, body)` — builds `HttpRequestMessage` with `Authorization: Bearer {token}`.

### Auth in Tests — IMPORTANT
JWT is emitted as `HttpOnly` cookie `gl_token`, **not** in the JSON body.

**To extract token from register/login response:**
```csharp
var cookieHeader = response.Headers.GetValues("Set-Cookie")
    .First(h => h.StartsWith("gl_token=", StringComparison.Ordinal));
var token = cookieHeader.Split(';')[0]["gl_token=".Length..];
```

**In integration tests** (endpoint tests with `IAsyncLifetime`):
```csharp
public async Task InitializeAsync()
{
    var (token, _) = await TestHelpers.RegisterAndLoginAsync(_client);
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
```
Bearer token is still accepted by the JWT middleware (fallback to `Authorization: Bearer` header when no cookie present).

### Test Coverage (69 total — all passing)
**Integration (39):**
- `AuthEndpointTests` — register/login verifies `Set-Cookie: gl_token` + userId in body (6)
- `GamesEndpointTests` — GetGameById (2)
- `PlatformsEndpointTests` — GetPlatforms (2)
- `ReleasesEndpointTests` — GetReleasesByMonth (4)
- `SyncTests` — idempotent sync, exclusive/multiplatform (4)
- `SocialEndpointTests` — favorites, purchases, status, groups, insights, members (15)

**Unit (30 — with NSubstitute):**
- `Unit/Domain/`: UserEntityTests, GroupEntityTests, GameFavoriteAndPurchaseEntityTests
- `Unit/Handlers/`: RegisterHandlerTests, LoginHandlerTests, JoinGroupHandlerTests, CreateGroupHandlerTests, GetGroupInsightsHandlerTests, GetMyGroupHandlerTests

### Test Naming
`MethodOrRoute_Scenario_ExpectedResult` in Spanish:
- `Register_ConCodigoValido_Devuelve200ConCookieYUserId`
- `GetReleasesByMonth_ConFiltroPlataforma_DevuelveLanzamientosFiltrados`
- `GetGameById_IdNoExistente_Devuelve404`

### Patterns
- **Arrange-Act-Assert** strictly.
- Each test is **fully independent** — no shared mutable state.
- Use `IClassFixture<CustomWebApplicationFactory>` to share factory.
- Social tests: use `TestHelpers.AuthRequest` per test (each needs its own user).
- Error messages in assertions must match Spanish backend messages (e.g., `"*no encontrado*"`).

### Token Optimization
- Short, precise responses. No repeated existing code.
- Minimize premium requests by combining related test work.
