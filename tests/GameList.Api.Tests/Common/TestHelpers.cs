using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace GameList.Api.Tests.Common;

/// <summary>
/// Métodos auxiliares compartidos entre los tests de integración.
/// Centraliza operaciones repetitivas como registro, login y construcción de requests autenticados.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Código de registro inyectado en la fábrica de tests.
    /// Debe coincidir con el valor configurado en <see cref="CustomWebApplicationFactory"/>.
    /// </summary>
    public const string RegistrationSecret = "TEST-SECRET";

    /// <summary>
    /// Registra un usuario con credenciales únicas basadas en UUID y devuelve el token JWT y el userId.
    /// El email y username son únicos por llamada, por lo que es seguro llamar a este método
    /// varias veces en el mismo test o entre tests que comparten la misma BD.
    /// </summary>
    public static async Task<(string Token, int UserId)> RegisterAndLoginAsync(HttpClient client)
    {
        var id = Guid.NewGuid().ToString("N")[..12];

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"user{id}",
            Email = $"user{id}@test.com",
            Password = "TestPass1!",
            InviteCode = RegistrationSecret
        });

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (
            json.GetProperty("token").GetString()!,
            json.GetProperty("userId").GetInt32()
        );
    }

    /// <summary>
    /// Construye un <see cref="HttpRequestMessage"/> con el header <c>Authorization: Bearer {token}</c>.
    /// Si se proporciona <paramref name="body"/>, se serializa como JSON en el cuerpo de la petición.
    /// </summary>
    public static HttpRequestMessage AuthRequest(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return request;
    }
}
