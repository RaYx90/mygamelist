using GameList.Application.Features.Auth.DTOs;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<UserDto?>;
