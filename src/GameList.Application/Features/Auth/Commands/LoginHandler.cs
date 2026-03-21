using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

/// <summary>
/// Handler MediatR que autentica a un usuario verificando sus credenciales.
/// </summary>
public sealed class LoginHandler : IRequestHandler<LoginCommand, UserDto?>
{
    private readonly IUserRepository userRepository;
    private readonly IPasswordHasher passwordHasher;

    /// <summary>
    /// Inicializa el handler con las dependencias necesarias.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="passwordHasher">Servicio de verificación de contraseñas.</param>
    public LoginHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Busca el usuario por email y verifica la contraseña. Devuelve <c>null</c> si las credenciales no son válidas.
    /// </summary>
    /// <param name="request">Datos del comando de login.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>DTO del usuario autenticado, o <c>null</c> si las credenciales son incorrectas.</returns>
    public async Task<UserDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;
        return new UserDto(user.Id, user.Username, user.Email, user.GroupId, user.AvatarPath);
    }
}
