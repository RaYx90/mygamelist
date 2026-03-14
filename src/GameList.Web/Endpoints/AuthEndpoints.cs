using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.Commands;
using GameList.Application.Features.Auth.Queries;
using GameList.Domain.Exceptions;
using GameList.Infrastructure.Auth;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GameList.Web.Endpoints;

/// <summary>
/// Endpoints Minimal API para autenticación: registro, inicio de sesión, sesión actual y logout.
/// El JWT se emite como cookie HttpOnly + SameSite=Strict para evitar acceso desde JS (XSS-safe).
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/auth").WithTags("Auth");
        api.MapPost("/register", Register).WithName("Register");
        api.MapPost("/login", Login).WithName("Login");
        api.MapGet("/me", Me).WithName("Me").RequireAuthorization();
        api.MapPost("/logout", Logout).WithName("Logout");
        api.MapPut("/username", ChangeUsername).WithName("ChangeUsername").RequireAuthorization();
        return app;
    }

    private static async Task<Results<Ok<object>, BadRequest<string>>> Register(
        ISender sender, IJwtTokenService jwtService,
        IOptions<RegistrationOptionsConfig> registrationOptions,
        IHostEnvironment env, HttpContext ctx,
        RegisterRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.InviteCode) ||
            body.InviteCode != registrationOptions.Value.SecretCode)
            return TypedResults.BadRequest("Código de acceso inválido.");

        try
        {
            var user = await sender.Send(new RegisterCommand(body.Username, body.Email, body.Password), ct);
            var token = jwtService.GenerateToken(user.UserId, user.Username, user.Email, user.GroupId);
            SetAuthCookie(ctx, token, env);
            return TypedResults.Ok((object)new { user.UserId, user.Username, user.Email, user.GroupId });
        }
        catch (DomainException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<Ok<object>, UnauthorizedHttpResult>> Login(
        ISender sender, IJwtTokenService jwtService,
        IHostEnvironment env, HttpContext ctx,
        LoginRequest body, CancellationToken ct)
    {
        var user = await sender.Send(new LoginCommand(body.Email, body.Password), ct);
        if (user is null) return TypedResults.Unauthorized();
        var token = jwtService.GenerateToken(user.UserId, user.Username, user.Email, user.GroupId);
        SetAuthCookie(ctx, token, env);
        return TypedResults.Ok((object)new { user.UserId, user.Username, user.Email, user.GroupId });
    }

    private static async Task<Results<Ok<object>, UnauthorizedHttpResult>> Me(
        ISender sender, ClaimsPrincipal user, CancellationToken ct)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dto = await sender.Send(new GetCurrentUserQuery(userId), ct);
        if (dto is null) return TypedResults.Unauthorized();
        return TypedResults.Ok((object)new { dto.UserId, dto.Username, dto.Email, dto.GroupId });
    }

    private static async Task<Results<Ok<object>, BadRequest<string>, Conflict<string>>> ChangeUsername(
        ISender sender, IJwtTokenService jwtService,
        IHostEnvironment env, HttpContext ctx,
        ClaimsPrincipal principal, ChangeUsernameRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.NewUsername) || body.NewUsername.Trim().Length < 3)
            return TypedResults.BadRequest("El nombre de usuario debe tener al menos 3 caracteres.");
        try
        {
            var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var newUsername = await sender.Send(new ChangeUsernameCommand(userId, body.NewUsername), ct);
            // Re-emitir JWT con el nuevo nombre
            var meDto = await sender.Send(new GetCurrentUserQuery(userId), ct);
            var token = jwtService.GenerateToken(userId, newUsername, meDto!.Email, meDto.GroupId);
            SetAuthCookie(ctx, token, env);
            return TypedResults.Ok((object)new { username = newUsername });
        }
        catch (ConflictException ex) { return TypedResults.Conflict(ex.Message); }
        catch (DomainException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static Ok Logout(HttpContext ctx)
    {
        ctx.Response.Cookies.Delete("gl_token");
        return TypedResults.Ok();
    }

    /// <summary>
    /// Escribe la cookie JWT: HttpOnly, SameSite=Lax, Secure solo si la conexión es HTTPS.
    /// SameSite=Lax permite que la cookie se envíe en recargas y navegaciones de nivel superior,
    /// evitando cierres de sesión inesperados en móvil.
    /// Secure se activa según el protocolo de la request para que funcione en HTTP (Docker local)
    /// y en HTTPS (producción).
    /// </summary>
    private static void SetAuthCookie(HttpContext ctx, string token, IHostEnvironment _env)
    {
        ctx.Response.Cookies.Append("gl_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = ctx.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/"
        });
    }
}

/// <summary>Cuerpo de la solicitud de registro de usuario.</summary>
public sealed record RegisterRequest(string Username, string Email, string Password, string InviteCode);

/// <summary>Cuerpo de la solicitud de inicio de sesión.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Cuerpo de la solicitud de cambio de nombre de usuario.</summary>
public sealed record ChangeUsernameRequest(string NewUsername);
