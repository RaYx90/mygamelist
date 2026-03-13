using System.Security.Cryptography;
using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Crea un nuevo grupo social, genera un código de invitación único
/// y añade automáticamente al creador como primer miembro.
/// </summary>
public sealed class CreateGroupHandler : IRequestHandler<CreateGroupCommand, GroupDto>
{
    private readonly IGroupRepository groupRepository;
    private readonly IUserRepository userRepository;

    public CreateGroupHandler(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        this.groupRepository = groupRepository;
        this.userRepository = userRepository;
    }

    public async Task<GroupDto> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        // Generación del código de invitación:
        //   1. Se generan 6 bytes aleatorios criptográficamente seguros (48 bits de entropía).
        //   2. Se codifican en Base64 → 8 caracteres (cada 6 bytes producen 8 chars en Base64).
        //   3. Se reemplazan los caracteres especiales de Base64 (+, /, =) por letras (A, B, C)
        //      para que el código sea URL-safe y fácil de escribir sin necesidad de escapado.
        //   4. Se toman los primeros 8 caracteres y se pasan a mayúsculas.
        // Resultado: código de 8 caracteres alfanuméricos, ej. "X7KQM2RA".
        var inviteCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(6))
            .Replace("+", "A").Replace("/", "B").Replace("=", "C")[..8].ToUpperInvariant();

        var group = GroupEntity.Create(request.GroupName, inviteCode);
        await groupRepository.AddAsync(group, cancellationToken);

        // Se guarda primero para que EF Core asigne group.Id antes de usarlo como FK en JoinGroup.
        await groupRepository.SaveChangesAsync(cancellationToken);

        // El creador se convierte automáticamente en el primer miembro del grupo.
        user.JoinGroup(group.Id);
        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new GroupDto(group.Id, group.Name, group.InviteCode, [user.Username]);
    }
}
