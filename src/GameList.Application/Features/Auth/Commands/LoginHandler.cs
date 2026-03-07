using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.DTOs;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

public sealed class LoginHandler : IRequestHandler<LoginCommand, UserDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;
        return new UserDto(user.Id, user.Username, user.Email, user.GroupId);
    }
}
