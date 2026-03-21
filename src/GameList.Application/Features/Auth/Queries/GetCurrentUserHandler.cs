using GameList.Application.Features.Auth.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Auth.Queries;

/// <summary>
/// Handler MediatR que resuelve <see cref="GetCurrentUserQuery"/> consultando la BD
/// para obtener siempre los datos actuales del usuario (incluido groupId actualizado).
/// </summary>
public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly IUserRepository userRepository;

    public GetCurrentUserHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return null;
        return new UserDto(user.Id, user.Username, user.Email, user.GroupId, user.AvatarPath);
    }
}
