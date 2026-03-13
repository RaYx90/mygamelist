using FluentAssertions;
using GameList.Api.Tests.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GameList.Api.Tests.Endpoints;

/// <summary>
/// Tests de integración para los endpoints de autenticación:
/// POST /api/auth/register y POST /api/auth/login.
/// </summary>
public sealed class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    // ── Registro ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ConCodigoValido_Devuelve200ConTokenYUserId()
    {
        var id = Guid.NewGuid().ToString("N")[..12];

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"user{id}",
            Email = $"user{id}@test.com",
            Password = "TestPass1!",
            InviteCode = TestHelpers.RegistrationSecret
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        json.GetProperty("userId").GetInt32().Should().BePositive();
        json.GetProperty("username").GetString().Should().StartWith("user");
    }

    [Fact]
    public async Task Register_ConCodigoInvalido_Devuelve400()
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "cualquieruser",
            Email = "cualquier@test.com",
            Password = "TestPass1!",
            InviteCode = "CODIGO-ERRONEO"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ConEmailDuplicado_Devuelve400()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        var email = $"dup{id}@test.com";

        // Primer registro — debe funcionar
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"user{id}a",
            Email = email,
            Password = "TestPass1!",
            InviteCode = TestHelpers.RegistrationSecret
        });

        // Segundo registro con el mismo email — debe fallar con 400
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"user{id}b",
            Email = email,
            Password = "TestPass1!",
            InviteCode = TestHelpers.RegistrationSecret
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ConCredencialesValidas_Devuelve200ConToken()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        var email = $"login{id}@test.com";
        const string password = "TestPass1!";

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"loginuser{id}",
            Email = email,
            Password = password,
            InviteCode = TestHelpers.RegistrationSecret
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Devuelve401()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        var email = $"badpass{id}@test.com";

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"badpassuser{id}",
            Email = email,
            Password = "CorrectPass1!",
            InviteCode = TestHelpers.RegistrationSecret
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = "WrongPass999!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ConEmailNoExistente_Devuelve401()
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "noexiste.nunca@test.com",
            Password = "TestPass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
