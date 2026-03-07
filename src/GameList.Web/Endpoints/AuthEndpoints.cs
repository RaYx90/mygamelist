using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.Commands;
using GameList.Domain.Exceptions;
using GameList.Infrastructure.Auth;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace GameList.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/auth").WithTags("Auth");
        api.MapPost("/register", Register).WithName("Register");
        api.MapPost("/login", Login).WithName("Login");
        return app;
    }

    private static async Task<Results<Ok<object>, BadRequest<string>>> Register(
        ISender sender, IJwtTokenService jwtService, IOptions<RegistrationOptionsConfig> registrationOptions,
        RegisterRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.InviteCode) ||
            body.InviteCode != registrationOptions.Value.SecretCode)
            return TypedResults.BadRequest("Código de acceso inválido.");

        try
        {
            var user = await sender.Send(new RegisterCommand(body.Username, body.Email, body.Password), ct);
            var token = jwtService.GenerateToken(user.UserId, user.Username, user.Email, user.GroupId);
            return TypedResults.Ok((object)new { token, user.UserId, user.Username, user.Email, user.GroupId });
        }
        catch (DomainException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<Ok<object>, UnauthorizedHttpResult>> Login(
        ISender sender, IJwtTokenService jwtService, LoginRequest body, CancellationToken ct)
    {
        var user = await sender.Send(new LoginCommand(body.Email, body.Password), ct);
        if (user is null) return TypedResults.Unauthorized();
        var token = jwtService.GenerateToken(user.UserId, user.Username, user.Email, user.GroupId);
        return TypedResults.Ok((object)new { token, user.UserId, user.Username, user.Email, user.GroupId });
    }
}

public sealed record RegisterRequest(string Username, string Email, string Password, string InviteCode);
public sealed record LoginRequest(string Email, string Password);
