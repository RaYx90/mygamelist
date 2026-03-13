using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Permite a un usuario unirse a un grupo existente usando su código de invitación.
/// </summary>
public sealed class JoinGroupHandler : IRequestHandler<JoinGroupCommand, GroupDto?>
{
    private readonly IGroupRepository groupRepository;
    private readonly IUserRepository userRepository;

    public JoinGroupHandler(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        this.groupRepository = groupRepository;
        this.userRepository = userRepository;
    }

    /// <summary>
    /// Intenta unir al usuario al grupo identificado por <see cref="JoinGroupCommand.InviteCode"/>.
    /// </summary>
    /// <returns>
    /// <see cref="GroupDto"/> con los datos del grupo si el usuario se unió correctamente;
    /// <c>null</c> si el código de invitación no corresponde a ningún grupo o el usuario no existe.
    /// <para>
    /// Devolver <c>null</c> en lugar de lanzar una excepción es una decisión de diseño deliberada:
    /// un código inválido es un escenario de entrada de usuario esperado, no una condición excepcional.
    /// El endpoint mapea <c>null</c> a 400 Bad Request; el frontend puede usar el GroupDto
    /// para actualizar el groupId en el store inmediatamente sin llamadas adicionales.
    /// </para>
    /// </returns>
    public async Task<GroupDto?> Handle(JoinGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetByInviteCodeAsync(request.InviteCode, cancellationToken);
        if (group is null) return null; // Código no encontrado — se trata como error de entrada del usuario.

        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return null; // No debería ocurrir en la práctica, pero se guarda por seguridad.

        user.JoinGroup(group.Id);
        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        // Se cargan los miembros actuales del grupo para construir el DTO completo.
        var members = await userRepository.GetByGroupIdAsync(group.Id, cancellationToken);
        return new GroupDto(group.Id, group.Name, group.InviteCode, members.Select(m => m.Username).ToList());
    }
}
