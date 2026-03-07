namespace GameList.Application.Features.Platforms.DTOs;

public sealed record PlatformDto(
    int Id,
    string Name,
    string Slug,
    string? Abbreviation
);
