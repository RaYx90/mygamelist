using GameList.Application.Features.Games.Queries;
using GameList.Application.Features.Platforms.Queries;
using GameList.Application.Features.Releases.Queries;
using GameList.Domain.Enums;
using GameList.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameList.Web.Endpoints;

/// <summary>
/// Endpoints Minimal API para el catálogo de juegos: lanzamientos por mes, plataformas y detalle de juego.
/// Todos requieren autenticación.
/// </summary>
public static class GameEndpoints
{
    /// <summary>
    /// Registra los endpoints del catálogo bajo la ruta /api, requiriendo autorización.
    /// </summary>
    /// <param name="app">Constructor de rutas de la aplicación.</param>
    /// <returns>El mismo <see cref="IEndpointRouteBuilder"/> para encadenamiento.</returns>
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").WithTags("GameList").RequireAuthorization();

        api.MapGet("/releases", GetReleasesByMonth)
            .WithName("GetReleasesByMonth");

        api.MapGet("/platforms", GetPlatforms)
            .WithName("GetPlatforms");

        api.MapGet("/games/{id:int}", GetGameDetail)
            .WithName("GetGameDetail");

        return app;
    }

    private static async Task<Ok<object>> GetReleasesByMonth(
        ISender sender,
        int? year,
        int? month,
        int? platformId,
        int? category,
        bool? isIndie,
        CancellationToken cancellationToken)
    {
        var currentYear = year ?? DateTime.UtcNow.Year;
        var currentMonth = month ?? DateTime.UtcNow.Month;
        var gameCategory = category.HasValue ? (GameCategoryEnum?)category.Value : null;

        var result = await sender.Send(
            new GetReleasesByMonthQuery(currentYear, currentMonth, platformId, gameCategory, isIndie),
            cancellationToken);

        return TypedResults.Ok((object)result);
    }

    private static async Task<Ok<object>> GetPlatforms(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPlatformsQuery(), cancellationToken);
        return TypedResults.Ok((object)result);
    }

    private static async Task<Results<Ok<object>, NotFound<string>>> GetGameDetail(
        ISender sender,
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new GetGameDetailQuery(id), cancellationToken);
            return TypedResults.Ok((object)result);
        }
        catch (GameNotFoundException ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
    }
}
