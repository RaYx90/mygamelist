using GameList.Application.Features.Social.Commands;
using GameList.Application.Features.Social.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace GameList.Web.Endpoints;

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
        return app;
    }

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

    private static async Task<Results<Ok<object>, BadRequest<string>>> JoinGroup(ISender sender, ClaimsPrincipal user, JoinGroupRequest body, CancellationToken ct)
    {
        var ok = await sender.Send(new JoinGroupCommand(GetUserId(user), body.InviteCode), ct);
        if (!ok) return TypedResults.BadRequest("Codigo de invitacion invalido.");
        return TypedResults.Ok((object)"ok");
    }
}

public sealed record CreateGroupRequest(string Name);
public sealed record JoinGroupRequest(string InviteCode);
