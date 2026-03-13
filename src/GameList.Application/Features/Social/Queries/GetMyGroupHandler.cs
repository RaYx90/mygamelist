using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Devuelve el grupo al que pertenece el usuario solicitante, incluyendo la lista completa de miembros.
/// Devuelve <c>null</c> si el usuario no pertenece a ningún grupo;
/// el frontend interpreta <c>null</c> como "sin grupo" y muestra los formularios de crear/unirse.
/// </summary>
public sealed class GetMyGroupHandler : IRequestHandler<GetMyGroupQuery, GroupDto?>
{
    private readonly IUserRepository userRepository;
    private readonly IGroupRepository groupRepository;

    public GetMyGroupHandler(IUserRepository userRepository, IGroupRepository groupRepository)
    {
        this.userRepository = userRepository;
        this.groupRepository = groupRepository;
    }

    public async Task<GroupDto?> Handle(GetMyGroupQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        // Salida rápida: el usuario no existe o no tiene grupo asignado.
        if (user?.GroupId is null) return null;

        var group = await groupRepository.GetByIdAsync(user.GroupId.Value, cancellationToken);
        if (group is null) return null; // Guarda de integridad — el FK GroupId siempre debería resolverse.

        // Se cargan los miembros por separado para obtener sus usernames para el DTO.
        var members = await userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
        var memberUsernames = members.Select(m => m.Username).ToList();

        return new GroupDto(group.Id, group.Name, group.InviteCode, memberUsernames);
    }
}
