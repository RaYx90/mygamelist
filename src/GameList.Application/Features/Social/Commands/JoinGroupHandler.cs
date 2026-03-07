using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

public sealed class JoinGroupHandler : IRequestHandler<JoinGroupCommand, bool>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public JoinGroupHandler(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(JoinGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByInviteCodeAsync(request.InviteCode, cancellationToken);
        if (group is null) return false;
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return false;
        user.JoinGroup(group.Id);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
