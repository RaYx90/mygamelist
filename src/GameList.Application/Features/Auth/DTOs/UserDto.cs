namespace GameList.Application.Features.Auth.DTOs;

public sealed record UserDto(int UserId, string Username, string Email, int? GroupId);
