using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.DTOs;
using GameList.Domain.Entities;
using GameList.Domain.Exceptions;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

/// <summary>
/// Handler MediatR que procesa el registro de un nuevo usuario, validando unicidad de email y nombre de usuario.
/// </summary>
public sealed class RegisterHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly IUserRepository userRepository;
    private readonly IPasswordHasher passwordHasher;

    /// <summary>
    /// Inicializa el handler con las dependencias necesarias.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="passwordHasher">Servicio de hashing de contraseñas.</param>
    public RegisterHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Valida que el email y nombre de usuario sean únicos, crea el usuario y devuelve su DTO.
    /// </summary>
    /// <param name="request">Datos del comando de registro.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>DTO con los datos del usuario recién creado.</returns>
    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ConflictException($"El email '{request.Email}' ya está registrado.");
        if (await userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new ConflictException($"El nombre de usuario '{request.Username}' ya está en uso.");

        var hash = passwordHasher.Hash(request.Password);
        var user = UserEntity.Create(request.Username, request.Email, hash);
        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Username, user.Email, user.GroupId);
    }
}
