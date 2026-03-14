using FluentAssertions;
using GameList.Api.Tests.Common;
using GameList.Application.Features.Sync.Commands;
using GameList.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GameList.Api.Tests.Endpoints;

/// <summary>
/// Tests de integración para los 10 endpoints de /api/social.
/// Implementa <see cref="IAsyncLifetime"/> para registrar un usuario único y obtener un gameId
/// válido antes de cada test, garantizando el aislamiento de datos entre tests.
/// </summary>
public sealed class SocialEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory factory;

    // Estado compartido inicializado antes de cada test por InitializeAsync.
    private string token = string.Empty;
    private int gameId;

    public SocialEndpointTests(CustomWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    /// <summary>
    /// Se ejecuta antes de cada test: registra un usuario único, sincroniza juegos (idempotente)
    /// y obtiene el ID de un juego existente para usar en los tests de favoritos/compras.
    /// </summary>
    public async Task InitializeAsync()
    {
        (token, _) = await TestHelpers.RegisterAndLoginAsync(client);

        using var scope = factory.Services.CreateScope();

        // Sync es idempotente — puede ejecutarse varias veces sin problema.
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));

        // Obtener cualquier juego de la BD para usarlo en los tests.
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        gameId = await db.Games.Select(g => g.Id).FirstAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Helper: construye un request autenticado con el token del test actual.
    private HttpRequestMessage Req(HttpMethod method, string url, object? body = null) =>
        TestHelpers.AuthRequest(method, url, token, body);

    // ── Favoritos ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddFavorite_ConToken_Devuelve200()
    {
        var response = await client.SendAsync(Req(HttpMethod.Post, $"/api/social/favorites/{gameId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveFavorite_DespuesDeAgregar_Devuelve200()
    {
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/favorites/{gameId}"));

        var response = await client.SendAsync(Req(HttpMethod.Delete, $"/api/social/favorites/{gameId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Compras ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkPurchased_ConToken_Devuelve200()
    {
        var response = await client.SendAsync(Req(HttpMethod.Post, $"/api/social/purchases/{gameId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnmarkPurchased_DespuesDeMarcar_Devuelve200()
    {
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/purchases/{gameId}"));

        var response = await client.SendAsync(Req(HttpMethod.Delete, $"/api/social/purchases/{gameId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Estado ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatus_ConFavoritoYCompra_DevuelveEstadoCorrecto()
    {
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/favorites/{gameId}"));
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/purchases/{gameId}"));

        var response = await client.SendAsync(Req(HttpMethod.Get, $"/api/social/status?gameIds={gameId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        // Las propiedades del DTO UserGameStatusDto se serializan en camelCase
        json.GetProperty("myFavorites").EnumerateArray()
            .Select(e => e.GetInt32()).Should().Contain(gameId);
        json.GetProperty("myPurchases").EnumerateArray()
            .Select(e => e.GetInt32()).Should().Contain(gameId);
    }

    [Fact]
    public async Task GetStatus_SinFavoritosNiCompras_DevuelveListasVacias()
    {
        var response = await client.SendAsync(Req(HttpMethod.Get, $"/api/social/status?gameIds={gameId}"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("myFavorites").EnumerateArray().Should().BeEmpty();
        json.GetProperty("myPurchases").EnumerateArray().Should().BeEmpty();
    }

    // ── Grupos ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGroup_ConToken_DevuelveGroupDtoConCodigoDe8Chars()
    {
        var response = await client.SendAsync(
            Req(HttpMethod.Post, "/api/social/groups", new { name = "Mi Grupo Test" }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetInt32().Should().BePositive();
        json.GetProperty("name").GetString().Should().Be("Mi Grupo Test");
        json.GetProperty("inviteCode").GetString().Should().HaveLength(8);
        json.GetProperty("memberUsernames").EnumerateArray().Should().HaveCount(1);
    }

    [Fact]
    public async Task JoinGroup_ConCodigoValido_DevuelveGroupDtoConMiembro()
    {
        // Crear el grupo con un usuario diferente — usar cliente separado para no contaminar la cookie del test
        var creatorClient = factory.CreateClient();
        var (creatorToken, _) = await TestHelpers.RegisterAndLoginAsync(creatorClient);
        var createResp = await creatorClient.SendAsync(
            TestHelpers.AuthRequest(HttpMethod.Post, "/api/social/groups", creatorToken, new { name = "Grupo Para Unirse" }));
        var groupJson = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var inviteCode = groupJson.GetProperty("inviteCode").GetString();

        // Unirse con el usuario del test
        var response = await client.SendAsync(
            Req(HttpMethod.Post, "/api/social/groups/join", new { inviteCode }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetInt32().Should().BePositive();
        json.GetProperty("inviteCode").GetString().Should().Be(inviteCode);
        // El DTO debe incluir al menos 2 miembros (creador + el que se une)
        json.GetProperty("memberUsernames").EnumerateArray().Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task JoinGroup_ConCodigoInvalido_Devuelve400()
    {
        var response = await client.SendAsync(
            Req(HttpMethod.Post, "/api/social/groups/join", new { inviteCode = "INVALIDO" }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetGroupInfo_UsuarioSinGrupo_DevuelveNull()
    {
        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/group/info"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // ASP.NET puede serializar null como "null" o cuerpo vacío dependiendo del serializador
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeOneOf(string.Empty, "null");
    }

    [Fact]
    public async Task GetGroupInfo_UsuarioConGrupo_DevuelveInfoDelGrupo()
    {
        var createResp = await client.SendAsync(
            Req(HttpMethod.Post, "/api/social/groups", new { name = "Grupo Info Test" }));
        var group = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var groupId = group.GetProperty("id").GetInt32();

        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/group/info"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetInt32().Should().Be(groupId);
        json.GetProperty("name").GetString().Should().Be("Grupo Info Test");
    }

    [Fact]
    public async Task GetGroupInsights_MismoUsuarioFavoritoYCompra_NoEsCoincidencia()
    {
        // La misma persona marca favorito Y compra → no es coincidencia (se necesitan personas distintas).
        await client.SendAsync(Req(HttpMethod.Post, "/api/social/groups", new { name = "Insights Test" }));
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/favorites/{gameId}"));
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/purchases/{gameId}"));

        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/group"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = (await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGroupMembers_UsuarioConGrupo_DevuelveListaDeMiembros()
    {
        await client.SendAsync(Req(HttpMethod.Post, "/api/social/groups", new { name = "Members Test" }));

        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/group/members"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = (await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        items.Should().HaveCountGreaterThanOrEqualTo(1);
        // Cada entrada debe tener username, favorites y purchases
        items[0].GetProperty("username").GetString().Should().NotBeNullOrEmpty();
    }

    // ── Mis favoritos y compras ───────────────────────────────────────────────

    [Fact]
    public async Task GetMyFavorites_ConFavoritosAgregados_DevuelveListaConJuego()
    {
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/favorites/{gameId}"));

        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/favorites"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = (await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        items.Should().HaveCountGreaterThanOrEqualTo(1);
        items.Should().Contain(e => e.GetProperty("gameId").GetInt32() == gameId);
    }

    [Fact]
    public async Task GetMyFavorites_SinFavoritos_DevuelveListaVacia()
    {
        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/favorites"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    [Fact]
    public async Task GetMyPurchases_ConComprasAgregadas_DevuelveListaConJuego()
    {
        await client.SendAsync(Req(HttpMethod.Post, $"/api/social/purchases/{gameId}"));

        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/purchases"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = (await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        items.Should().HaveCountGreaterThanOrEqualTo(1);
        items.Should().Contain(e => e.GetProperty("gameId").GetInt32() == gameId);
    }

    [Fact]
    public async Task GetMyPurchases_SinCompras_DevuelveListaVacia()
    {
        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/purchases"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    // ── Guards de autenticación ───────────────────────────────────────────────

    [Fact]
    public async Task SocialEndpoints_SinToken_Devuelve401()
    {
        // Usar cliente fresco sin cookies para verificar que el endpoint requiere autenticación
        var freshClient = factory.CreateClient();
        var response = await freshClient.GetAsync("/api/social/group/info");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddFavorite_DobleVez_EsIdempotente()
    {
        // Añadir el mismo favorito dos veces debe ser idempotente (el handler lo verifica antes de insertar).
        var first = await client.SendAsync(Req(HttpMethod.Post, $"/api/social/favorites/{gameId}"));
        var second = await client.SendAsync(Req(HttpMethod.Post, $"/api/social/favorites/{gameId}"));

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetGroupMembers_UsuarioSinGrupo_DevuelveListaVacia()
    {
        // El usuario del test no tiene grupo — GetGroupMembers debe devolver [] sin error.
        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/group/members"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    [Fact]
    public async Task GetGroupInsights_UsuarioSinGrupo_DevuelveListaVacia()
    {
        // El usuario no tiene grupo — insights debe devolver [] sin error.
        var response = await client.SendAsync(Req(HttpMethod.Get, "/api/social/group"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }
}
