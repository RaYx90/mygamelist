using GameList.Domain.Exceptions;
using GameList.Domain.Interfaces;
using MediatR;
#pragma warning disable CS1591

namespace GameList.Application.Features.Auth.Commands;

/// <summary>
/// Cambia el nombre de usuario, validando que el nuevo nombre no esté ya en uso.
/// Devuelve el nuevo nombre para que el endpoint pueda re-emitir el JWT.
/// </summary>
public sealed class ChangeUsernameHandler : IRequestHandler<ChangeUsernameCommand, string>
{
    private readonly IUserRepository userRepository;

    public ChangeUsernameHandler(IUserRepository userRepository)
        => this.userRepository = userRepository;

    public async Task<string> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new ConflictException("Usuario no encontrado.");

        if (await userRepository.ExistsByUsernameAsync(request.NewUsername, cancellationToken)
            && !user.Username.Equals(request.NewUsername, StringComparison.OrdinalIgnoreCase))
            throw new ConflictException($"El nombre de usuario '{request.NewUsername}' ya está en uso.");

        user.ChangeUsername(request.NewUsername);
        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return user.Username;
    }
}
