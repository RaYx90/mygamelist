using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.DTOs;
using GameList.Domain.Entities;
using GameList.Domain.Exceptions;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ConflictException($"El email '{request.Email}' ya esta registrado.");
        if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new ConflictException($"El nombre de usuario '{request.Username}' ya esta en uso.");

        var hash = _passwordHasher.Hash(request.Password);
        var user = UserEntity.Create(request.Username, request.Email, hash);
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Username, user.Email, user.GroupId);
    }
}
