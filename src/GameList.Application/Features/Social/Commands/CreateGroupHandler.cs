using System.Security.Cryptography;
using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Entities;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

public sealed class CreateGroupHandler : IRequestHandler<CreateGroupCommand, GroupDto>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public CreateGroupHandler(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    public async Task<GroupDto> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        var inviteCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(6))
            .Replace("+", "A").Replace("/", "B").Replace("=", "C")[..8].ToUpperInvariant();

        var group = GroupEntity.Create(request.GroupName, inviteCode);
        await _groupRepository.AddAsync(group, cancellationToken);
        await _groupRepository.SaveChangesAsync(cancellationToken);

        user.JoinGroup(group.Id);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new GroupDto(group.Id, group.Name, group.InviteCode, [user.Username]);
    }
}
