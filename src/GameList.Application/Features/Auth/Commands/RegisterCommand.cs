using GameList.Application.Features.Auth.DTOs;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

public sealed record RegisterCommand(string Username, string Email, string Password)
    : IRequest<UserDto>;
