using GameList.Application.Features.Social.Commands;
using GameList.Application.Features.Social.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace GameList.Web.Endpoints;

/// <summary>
/// Endpoints Minimal API para las funcionalidades sociales de la aplicación.
/// Todos requieren autenticación JWT (RequireAuthorization).
/// </summary>
/// <remarks>
/// Tabla de endpoints:
/// <code>
/// POST   /api/social/favorites/{gameId}   — Añade un juego a favoritos
/// DELETE /api/social/favorites/{gameId}   — Elimina un juego de favoritos
/// POST   /api/social/purchases/{gameId}   — Marca un juego como comprado
/// DELETE /api/social/purchases/{gameId}   — Desmarca un juego como comprado
/// GET    /api/social/status?gameIds=1,2,3 — Estado de favoritos/compras del usuario para los juegos indicados
/// GET    /api/social/group                — Insights del grupo (juegos deseados/comprados por los miembros)
/// GET    /api/social/group/info           — Información básica del grupo del usuario (nombre, miembros, código)
/// GET    /api/social/group/members        — Listas de favoritos y compras por miembro del grupo
/// POST   /api/social/groups               — Crea un nuevo grupo
/// POST   /api/social/groups/join          — Se une a un grupo existente con código de invitación
/// </code>
/// </remarks>
public static class SocialEndpoints
{
    public static IEndpointRouteBuilder MapSocialEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/social").WithTags("Social").RequireAuthorization();
        api.MapPost("/favorites/{gameId:int}", AddFavorite);
        api.MapDelete("/favorites/{gameId:int}", RemoveFavorite);
        api.MapPost("/purchases/{gameId:int}", MarkPurchased);
        api.MapDelete("/purchases/{gameId:int}", UnmarkPurchased);
        api.MapGet("/status", GetStatus);
        api.MapGet("/group", GetGroupInsights);
        api.MapGet("/group/info", GetMyGroup);
        api.MapGet("/group/members", GetGroupMembersGames);
        api.MapPost("/groups", CreateGroup);
        api.MapPost("/groups/join", JoinGroup);
        api.MapGet("/favorites", GetMyFavorites);
        api.MapGet("/purchases", GetMyPurchases);
        return app;
    }

    // Helper: extrae el userId del claim NameIdentifier del JWT.
    // Se asume que el claim siempre existe porque el endpoint requiere autenticación.
    private static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static async Task<Ok<object>> AddFavorite(ISender sender, int gameId, ClaimsPrincipal user, CancellationToken ct)
    { await sender.Send(new AddFavoriteCommand(GetUserId(user), gameId), ct); return TypedResults.Ok((object)"ok"); }

    private static async Task<Ok<object>> RemoveFavorite(ISender sender, int gameId, ClaimsPrincipal user, CancellationToken ct)
    { await sender.Send(new RemoveFavoriteCommand(GetUserId(user), gameId), ct); return TypedResults.Ok((object)"ok"); }

    private static async Task<Ok<object>> MarkPurchased(ISender sender, int gameId, ClaimsPrincipal user, CancellationToken ct)
    { await sender.Send(new MarkPurchasedCommand(GetUserId(user), gameId), ct); return TypedResults.Ok((object)"ok"); }

    private static async Task<Ok<object>> UnmarkPurchased(ISender sender, int gameId, ClaimsPrincipal user, CancellationToken ct)
    { await sender.Send(new UnmarkPurchasedCommand(GetUserId(user), gameId), ct); return TypedResults.Ok((object)"ok"); }

    private static async Task<Ok<object>> GetStatus(ISender sender, ClaimsPrincipal user, string? gameIds, CancellationToken ct)
    {
        // gameIds llega como string CSV "1,2,3" desde el query param. Se parsea con TryParse
        // para ignorar valores no numéricos sin lanzar excepción.
        var ids = gameIds?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList() ?? [];
        var result = await sender.Send(new GetUserGameStatusQuery(GetUserId(user), ids), ct);
        return TypedResults.Ok((object)result);
    }

    private static async Task<Ok<object>> GetGroupInsights(ISender sender, ClaimsPrincipal user, CancellationToken ct)
    {
        var result = await sender.Send(new GetGroupInsightsQuery(GetUserId(user)), ct);
        return TypedResults.Ok((object)result);
    }

    private static async Task<Ok<object?>> GetMyGroup(ISender sender, ClaimsPrincipal user, CancellationToken ct)
    {
        var result = await sender.Send(new GetMyGroupQuery(GetUserId(user)), ct);
        return TypedResults.Ok<object?>(result);
    }

    private static async Task<Ok<object>> GetGroupMembersGames(ISender sender, ClaimsPrincipal user, CancellationToken ct)
    {
        var result = await sender.Send(new GetGroupMembersGamesQuery(GetUserId(user)), ct);
        return TypedResults.Ok((object)result);
    }

    private static async Task<Ok<object>> CreateGroup(ISender sender, ClaimsPrincipal user, CreateGroupRequest body, CancellationToken ct)
    {
        var result = await sender.Send(new CreateGroupCommand(GetUserId(user), body.Name), ct);
        return TypedResults.Ok((object)result);
    }

    // Devuelve GroupDto (igual que CreateGroup) para que el frontend pueda guardar el groupId en el store.
    // Antes devolvía "ok" → group.id era undefined y el store quedaba roto tras el join.
    private static async Task<Results<Ok<object>, BadRequest<string>>> JoinGroup(ISender sender, ClaimsPrincipal user, JoinGroupRequest body, CancellationToken ct)
    {
        var result = await sender.Send(new JoinGroupCommand(GetUserId(user), body.InviteCode), ct);
        if (result is null) return TypedResults.BadRequest("Código de invitación inválido.");
        return TypedResults.Ok((object)result);
    }

    private static async Task<Ok<object>> GetMyFavorites(ISender sender, ClaimsPrincipal user, CancellationToken ct)
    {
        var result = await sender.Send(new GetMyFavoritesQuery(GetUserId(user)), ct);
        return TypedResults.Ok((object)result);
    }

    private static async Task<Ok<object>> GetMyPurchases(ISender sender, ClaimsPrincipal user, CancellationToken ct)
    {
        var result = await sender.Send(new GetMyPurchasesQuery(GetUserId(user)), ct);
        return TypedResults.Ok((object)result);
    }
}

public sealed record CreateGroupRequest(string Name);
public sealed record JoinGroupRequest(string InviteCode);
