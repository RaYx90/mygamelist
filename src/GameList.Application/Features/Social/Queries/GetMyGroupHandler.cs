using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

public sealed class GetMyGroupHandler : IRequestHandler<GetMyGroupQuery, GroupDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;

    public GetMyGroupHandler(IUserRepository userRepository, IGroupRepository groupRepository)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
    }

    public async Task<GroupDto?> Handle(GetMyGroupQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user?.GroupId is null) return null;

        var group = await _groupRepository.GetByIdAsync(user.GroupId.Value, cancellationToken);
        if (group is null) return null;

        var members = await _userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
        var memberUsernames = members.Select(m => m.Username).ToList();

        return new GroupDto(group.Id, group.Name, group.InviteCode, memberUsernames);
    }
}
